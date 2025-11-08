using CMSAPI.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Extensions
{
	public static class WebApplicationExtensions
	{
		public static WebApplication ConfigureApplication(this WebApplication app)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}
			var contentRoot = app.Environment.ContentRootPath;

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseStaticFiles();
			app.UseResponseCaching();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapControllers();

			app.UseMiddleware<ModelBindingErrorMiddleware>();

			return app;
		}
	}
}
