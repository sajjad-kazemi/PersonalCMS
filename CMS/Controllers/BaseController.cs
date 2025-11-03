using CMSAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CMSAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BaseController : Controller
	{
		public override OkResult Ok()
		{
			return new ApiOkResult();
		}
		public override OkObjectResult Ok([ActionResultObjectValue] object? result)
		{
			var response = ApiResponse.Succeed(result);
			return base.Ok(response);
		}
		protected ObjectResult Fail(string message)
		{
			var response = ApiResponse.Fail(message);
			return base.StatusCode(200,response);
		}
		protected ObjectResult Fail(int statusCode, string message)
		{
			var response = ApiResponse.Fail(message);
			return base.StatusCode(statusCode,response);
		}
	}
}
