using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.Class;
using System.Web;

namespace LocomotionEngines
{
	public class UserDataEngine
	{
		private static UserDataEngine instance;
		public static UserDataEngine getInstance()
		{
			if(instance == null)
				instance = new UserDataEngine();
			return instance;
		}
		private UserDataEngine() {}

		public User GetCurrentUser(HttpContextBase context)
		{
			using(DataModelContext ctx = new DataModelContext())
			{
				foreach(var u in ctx.Users.Where(
					u => u.Username == context.User.Identity.Name
				))
				{
					return u;
				}
			}
			return null;
		}

		public User GetCurrentUser(DataModelContext c, HttpContextBase context)
		{
			foreach(var u in c.Users.Where(
				u => u.Username == context.User.Identity.Name
			))
			{
				return u;
			}
			return null;
		}
	}
}
