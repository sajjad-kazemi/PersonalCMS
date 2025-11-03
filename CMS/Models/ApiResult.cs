using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace CMSAPI.Models
{

	public class ApiOkResult : OkResult
	{
		private const string Message = "درخواست شما با موفقیت انجام شد";

		public override void ExecuteResult(ActionContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var rsp = ApiResponse.Succeed(Message);
			context.HttpContext.Response.StatusCode = StatusCode;
			context.HttpContext.Response.WriteAsync(rsp.ToString());
		}
	}
	public class ApiResult : ApiResult<object>
	{
		public ApiResult()
		{
		}

		public ApiResult(bool isValid , string message) : base(isValid , message)
		{
		}

		public ApiResult(ExceptionCodeEnum exceptionCode , object data) : base(exceptionCode , data)
		{
		}

		public ApiResult(bool isValid , string message , object data) : base(isValid , message , data)
		{
		}
		public ApiResult(bool isValid , string message , object data , ExceptionCodeEnum code) : base(isValid , message , data , code)
		{
		}
	}
	public class ApiResult<T>
	{
		public ApiResult() : this(false , string.Empty)
		{
		}

		public ApiResult(bool isValid , string message)
		{
			isValid = isValid;
			Message = message;
		}

		public ApiResult(ExceptionCodeEnum exceptionCode) : this(false , string.Empty)
		{
			ExceptionCode = exceptionCode;
		}
		public ApiResult(ExceptionCodeEnum exceptionCode , T data) : this(exceptionCode)
		{
			Data = data;
		}

		public ApiResult(bool isValid , string message , T data) : this(isValid , message)
		{
			Data = data;
		}
		public ApiResult(bool isValid , string message , T data , ExceptionCodeEnum code) : this(isValid , message , data)
		{
			ExceptionCode = code;
		}

		public ExceptionCodeEnum? ExceptionCode { get; set; }

		public bool isValid { get; set; }

		[Obsolete]
		public string Message { get; set; }
		[Obsolete]
		public List<string> Errors { get; set; } = new List<string>();

		public T? Data { get; set; }
		public void ChangeState(ExceptionCodeEnum exceptionCode , T data = default)
		{
			isValid = false;
			ExceptionCode = exceptionCode;
			if (data != null)
			{
				Data = data;
			}
		}
		[Obsolete]
		public void ChangeState(bool isValid , string message = "" , T data = default)
		{
			isValid = isValid;
			Message = message;
			if (data != null)
			{
				Data = data;
			}
		}
	}
	public class ApiResponse : ApiResult
	{
		public static ApiResponse Succeed(string message)
		{
			return new ApiResponse
			{
				Message = message ,
				isValid = true
			};
		}
		public static ApiResponse Succeed(object? result)
		{
			return new ApiResponse
			{
				Data = result ,
				isValid = true
			};
		}
		public static ApiResponse Succeed(string message , object result)
		{
			return new ApiResponse
			{
				Data = result ,
				Message = message ,
				isValid = true
			};
		}

		public static ApiResponse Fail(string message)
		{
			return new ApiResponse
			{
				Message = message ,
				isValid = false
			};
		}
		public static ApiResponse Fail(ExceptionCodeEnum? code , string message)
		{
			return new ApiResponse
			{
				ExceptionCode = code ,
				Message = message ,
				isValid = false
			};
		}

		public override string ToString()
		{
			return JsonSerializer.Serialize(this , new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
		}
	}
	public enum ExceptionCodeEnum
	{
		UnknownError = 0,
		ModelNotValid = 1,
		ServerNotResponding = 2,
		AccessDenied = 3,
		IncorrectParameters = 4,
		CanNotPublish = 5,
	}
}
