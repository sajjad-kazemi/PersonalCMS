

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Services
{
	public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
	{
		private readonly AuthorizationMiddlewareResultHandler _resultHandler = new AuthorizationMiddlewareResultHandler();

		public async Task HandleAsync(RequestDelegate next , HttpContext context , AuthorizationPolicy policy , PolicyAuthorizationResult authorizeResult)
		{
			if (authorizeResult.Challenged)
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				context.Response.ContentType = "application/json";

				var payload = JsonSerializer.Serialize(new
				{
					error = "Unauthorized" ,
					message = "you must be authenticated to access this resource."
				});

				await context.Response.WriteAsync(payload);
				return;
			}

			if (authorizeResult.Forbidden)
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				context.Response.ContentType = "application/json";

				var payload = JsonSerializer.Serialize(new
				{
					error = "Forbidden" ,
					message = "You don't have permission to access to this resource"
				});

				await context.Response.WriteAsync(payload);
				return;
			}

			await _resultHandler.HandleAsync(next, context, policy , authorizeResult);
		}
	}
}
