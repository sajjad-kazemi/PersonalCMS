using CMSAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.User;
using Services;
using System.Security.Claims;

namespace CMS.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class UserController : BaseController
	{
		private readonly IUserService _userService;
		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> GetCurrentUser()
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
			var response = await _userService.GetUser(new GetUserRequest
			{
				Id = userId
			});
			return Ok(response);
		}

		[HttpPost]
		public async Task<IActionResult> GetUser(GetUserRequest request)
		{
			var response = await _userService.GetUser(request);
			return Ok(response);
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterRequest request)
		{
			var response = await _userService.Register(request);
			return Ok(response);
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginRequest request)
		{
			var response = await _userService.Login(request);
			return Ok(response.Data);
		}
	}
}
