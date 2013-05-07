using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataModel.Class;

namespace LocomotionWebApp.Models.Account
{
	public class LoginModel
	{
		public User NewUser { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string PassConfirm { get; set; }
		public string PermissionCode { get; set; }
		public DataModel.Enum.UserAccessGroup department { get; set; }
	}
}