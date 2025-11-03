using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class ResponseBase
	{
		public ResponseBase(bool isSuccess , string message = null)
		{
			if (message is null && !isSuccess)
			{
				message = "عملیات با خطا مواجه شد.";
			}
			Message = message;
			IsSuccess = isSuccess;
		}
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
	}
}
