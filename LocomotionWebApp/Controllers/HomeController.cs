using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataModel.Class;
using LocomotionEngines;
using LocomotionWebApp.Models.ViewModels;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Data.Entity.Validation;
using System.Web.Security;

namespace LocomotionWebApp.Controllers
{
	public class HomeController : Controller
	{
		//
		// GET: /Home/

		[Authorize]
		public ActionResult Index()
		{
			var hvm = new HomeViewModel();

			using(var c = new DataModelContext())
			{
				User currentUser = UserDataEngine.getInstance().GetCurrentUser(c, HttpContext);

				if(currentUser == null)
				{
					FormsAuthentication.SignOut();
					return RedirectToAction("Index", "Account");
				}

				hvm.Networks = c.Networks.Include("Author").Where(n =>
					n.Name != null &&
					n.Author.Username == currentUser.Username
					).ToList();
			}

			return View(hvm);
		}


	}
}
