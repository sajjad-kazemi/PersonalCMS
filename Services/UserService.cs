using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models.User;


namespace Services
{
	public interface IUserService
	{
		Task<GetUserResponse> GetCurrentUser();
		Task<List<GetUserResponse>> GetUser(GetUserRequest request);
	}
	public class UserService : IUserService
	{
		private readonly ApplicationDbContext _context;
		public UserService(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task<GetUserResponse> GetCurrentUser()
		{
			return new GetUserResponse()
			{
				Id = 3,
				FirstName = "test",
				LastName = "2",
				UserName = "test User",
			};
		}
		public async Task<List<GetUserResponse>> GetUser(GetUserRequest request)
		{
			var query = _context.User.AsQueryable();
			var hasFilter = false;
			if(request.Id is not null)
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
			}).ToListAsync();
		}
	}
}
