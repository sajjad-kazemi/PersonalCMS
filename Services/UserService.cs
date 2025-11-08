using Data;
using Data.Entities;
using Data.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.User;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Services
{
	public interface IUserService
	{
		Task<List<GetUserResponse>> GetUser(GetUserRequest request);
		Task<ResponseBase> Register(RegisterRequest request);
		Task<ResponseBase> Login(LoginRequest request);
	}
	public class UserService : IUserService
	{
		private readonly ApplicationDbContext _context;
		private readonly IPasswordHasher _passwordHasher;
		private readonly IConfiguration _configuration;
		public UserService(ApplicationDbContext context , IPasswordHasher passwordHasher , IConfiguration configuration)
		{
			_context = context;
			_passwordHasher = passwordHasher;
			_configuration = configuration;
		}

		public async Task<List<GetUserResponse>> GetUser(GetUserRequest request)
		{
			var query = _context.Users.AsQueryable();
			var hasFilter = false;
			if (request.Id is not null)
			{
				query = query.Where(x => x.Id == request.Id);
				hasFilter = true;
			}
			if (!request.UserName.IsNullOrEmpty())
			{
				query = query.Where(x => x.UserName.Contains(request.UserName));
				hasFilter = true;
			}
			if (!hasFilter)
			{
				query = query.Take(0);
			}
			return await query.Select(x => new GetUserResponse
			{
				Id = x.Id ,
				FirstName = x.FirstName ,
				LastName = x.LastName ,
				UserName = x.UserName ,
				MobileNumber = x.MobileNumber ,
				Email = x.Email ,
			}).ToListAsync();
		}

		public async Task<ResponseBase> UpdateUserInfo(UpdateUserInfoRequest request)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
			user.FirstName = request.FirstName;
			user.LastName = request.LastName;
			user.MobileNumber = request.MobileNumber;
			user.Email = request.Email;
			//user.Role = request.Role;

			_context.Update(user);
			await _context.SaveChangesAsync();
			return new ResponseBase(true);
		}

		public async Task<ResponseBase> Register(RegisterRequest request)
		{
			if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
			{
				return new ResponseBase(false);
			}
			var user = new User
			{
				UserName = request.UserName ,
				Password = _passwordHasher.HashPassword(request.Password) ,
			};
			_context.Users.Add(user);
			await _context.SaveChangesAsync();
			return new ResponseBase(true);
		}

		public async Task<ResponseBase> Login(LoginRequest request)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
			if (user is null)
			{
				return new ResponseBase(false);
			}
			var verifyPassword = _passwordHasher.VerifyHashedPassword(user.Password , request.Password);
			if (verifyPassword == PasswordVerificationResult.Failed)
			{
				return new ResponseBase(false);
			}
			var token = GenerateJwtToken(user);
			return new ResponseBase(true)
			{
				Data = new LoginResponse { Token = token }
			};
		}

		private string GenerateJwtToken(User user)
		{
			var jwtConfig = _configuration.GetSection("Jwt");

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, user.Email ?? ""),
				new Claim(ClaimTypes.Role, user.Role is null ? UserRole.USER.ToString() : user.Role.ToString()),
				new Claim("FirstName", user.FirstName ?? ""),
				new Claim("LastName", user.LastName ?? "")
			};

			if (user.Role is null)
			{
				claims.Add(new Claim(ClaimTypes.Role , user.Role.ToString()));
			}

			var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]));
			var creds = new SigningCredentials(signingKey , SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: jwtConfig["Issuer"] ,
				audience: jwtConfig["Audience"] ,
				claims: claims ,
				expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtConfig["LifeTimeInMinutes"] ?? "60")) ,
				signingCredentials: creds
				);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
