using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Services.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services ,
			WebApplicationBuilder builder)
		{
			services.AddControllers();
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
			services.AddResponseCompression(options =>
			{
				options.EnableForHttps = true;
				options.Providers.Add<GzipCompressionProvider>();
			});
			services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
			services.Configure<GzipCompressionProviderOptions>(options =>
			{
				options.Level = System.IO.Compression.CompressionLevel.Fastest;
			});
			
			var cmsConnectionString = builder.Configuration.GetConnectionString("CMS");
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer(cmsConnectionString ,
					serverDbContextOptionsBuilder =>
					{
						var minutes = (int)TimeSpan.FromMinutes(3).TotalSeconds;
						serverDbContextOptionsBuilder.CommandTimeout(minutes);
					});
			});

			//services.AddAuthentication(options =>
			//{
			//	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			//	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			//})
			//	.AddJwtBearer(options =>
			//	{
			//		options.TokenValidationParameters = new TokenValidationParameters()
			//		{
			//			ValidateIssuer = true,
			//			ValidIssuer = 
			//		}
			//	})

			services.AddControllers()
				.ConfigureApiBehaviorOptions(options =>
				{
					options.InvalidModelStateResponseFactory = context =>
					{
						var errors = context.ModelState
							.Where(ms => ms.Value.Errors.Count > 0)
							.Select(ms =>
							{
								var field = ms.Key;
								var errorMessages = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray();
								return new { field , errorMessages };
							}).ToArray();
						var result = new
						{
							status = 400 ,
							erorrs = errors
						};
						return new BadRequestObjectResult(result);
					};
				});

			// add services here
			//services.AddScoped<>();

			services.AddScoped<IUserService , UserService>();

			return services;
		}
	}
}
