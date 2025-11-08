using Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

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

			var jwtSettings = builder.Configuration.GetSection("JWT");
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
				{
					options.RequireHttpsMetadata = true;
					options.SaveToken = true;
					options.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidateIssuer = true ,
						ValidIssuer = jwtSettings["Issuer"] ,

						ValidateAudience = true ,
						ValidAudience = jwtSettings["Audience"] ,

						ValidateIssuerSigningKey = true ,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])) ,

						ValidateLifetime = true ,
						ClockSkew = TimeSpan.FromMinutes(1) ,
					};

					options.Events = new JwtBearerEvents
					{
						OnChallenge = async context =>
						{
							context.HandleResponse();
							var response = context.Response;
							response.StatusCode = StatusCodes.Status401Unauthorized;
							response.ContentType = "application/json";

							var payload = JsonSerializer.Serialize(new
							{
								error = "Unauthorized" ,
								message = "Authentication Token is missing or invalid."
							});

							await response.WriteAsync(payload);
						},
						OnForbidden = async context =>
						{
							var response = context.Response;
							response.StatusCode = StatusCodes.Status403Forbidden;
							response.ContentType = "application/json";

							var payload = JsonSerializer.Serialize(new
							{
								error = "Forbidden" ,
								message = "You Don't have permission to do this."
							});

							await response.WriteAsync(payload);
						}
					};
				});

			services.AddAuthorization();

			services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

			// swagger jwt
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1" , new OpenApiInfo { Title = "CMS API" , Version = "v1" });

				var bearerScheme = new OpenApiSecurityScheme
				{
					Name = "Authorization" ,
					Description = "Enter 'Bearer' [space] and then your token.\nExample: \"Bearer eyJhbGciOi...\"" ,
					In = ParameterLocation.Header ,
					Type = SecuritySchemeType.Http ,
					Scheme = "bearer" ,
					BearerFormat = "JWT" ,
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme ,
						Id = JwtBearerDefaults.AuthenticationScheme // "Bearer"
					}
				};

				c.AddSecurityDefinition(bearerScheme.Reference.Id , bearerScheme);

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{ bearerScheme, Array.Empty<string>() }
				});
			});

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

			services.AddScoped<IUserService , UserService>();
			services.AddScoped<IPasswordHasher , PasswordHasher>();

			return services;
		}
	}
}
