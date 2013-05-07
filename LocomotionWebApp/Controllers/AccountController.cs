using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LocomotionEngines;
using DataModel.Class;
using LocomotionWebApp.Models.ViewModels;
using System.Web.Security;
using LocomotionWebApp.Models.Account;
using System.Security.Cryptography;

namespace LocomotionWebApp.Controllers
{
	public class AccountController : Controller
	{
		//
		// GET: /Account/

		//public ActionResult Index()
		//{
		//	return View(new LoginModel());
		//}

		public ActionResult Index(LoginModel model, string returnUrl)
		{
			// If the user hasn't tried to log in yet, let them try.
			if(model.Username == null)
				return View(model);

			using(DataModelContext ctx = new DataModelContext())
			{
				foreach(var u in ctx.Users.Where(u => u.Username == model.Username))
				{
					byte[] salt = u.PasswordSalt;

					byte[] hash = GenerateSaltedHash(
						System.Text.Encoding.UTF8.GetBytes(model.Password), salt);

					if(u.PasswordHash.SequenceEqual(hash))
					{
						FormsAuthentication.SetAuthCookie(model.Username, false);
						if(returnUrl != null)
							return Redirect(returnUrl);
						return RedirectToAction("Index", "Home");
					}
				}
				ModelState.AddModelError("result", "Incorrect username or password. Try again.");
			}
			return View(model);
		}

		//
		// GET: /Account/Details/5

		public ActionResult Details(int id)
		{
			return View();
		}

		//
		// GET: /Account/Create

		public ActionResult Create()
		{
			return View();
		}

		//
		// POST: /Account/Create

		[HttpPost]
		public ActionResult Create(FormCollection collection, LoginModel model)
		{
			try
			{
				if(ModelState.IsValid && model.PassConfirm.Equals(model.Password))
				{
					using(DataModelContext ctx = new DataModelContext())
					{
						if(model.PermissionCode != "a95e")
						{
							ModelState.AddModelError("regResult", "Permission code invalid.");
							return View("Index", model);
						}
						if (ctx.Users.Any(o => o.Username == model.NewUser.Username))
						{
							ModelState.AddModelError("regResult", "Username already exists.");
							return View("Index", model);
						}
						else
						{
							byte[] salt = GenerateSalt(16);

							byte[] hash = GenerateSaltedHash(
								System.Text.Encoding.UTF8.GetBytes(model.Password), salt);
							
							ctx.Users.Add(new User()
							{
								EmailAddress = model.NewUser.EmailAddress,
								RealName = model.NewUser.RealName,
								Username = model.NewUser.Username,
								LastLogin = System.DateTime.Now,
								AccessGroup = model.department,
								PasswordSalt = salt,
								PasswordHash = hash,
							});
							
							ctx.SaveChanges();
							FormsAuthentication.SetAuthCookie(model.NewUser.Username, false);

							return RedirectToAction("Index", "Home");
						}
					}
				}
				else
				{
					ModelState.AddModelError("regResult", "Password does not match confirmation.");
					return View("Index", model);
				}

			}
			catch
			{
				return RedirectToAction("Index", "Home");
			}
		}

		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			return RedirectToAction("Index");
		}

		public ActionResult Name()
		{
			var nm = new NameModel();
			nm.Name = UserDataEngine.getInstance().GetCurrentUser(HttpContext).RealName;
			return View(nm);
		}

		public String Access()
		{
			var ac = new AccessModel();
			ac.department = UserDataEngine.getInstance().GetCurrentUser(HttpContext).AccessGroup;
			return ac.department.ToString();
		
		}

		/// <summary>
		/// Generates a length-byte random salt.
		/// </summary>
		static byte[] GenerateSalt(int length)
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			byte[] salt = new byte[length];

			rng.GetBytes(salt);

			return salt;
		}

		/// <summary>
		/// Computes the SHA256 salted hash using the given password and salt.
		/// Code originally from:
		/// http://stackoverflow.com/a/2138588/382780
		/// </summary>
		static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
		{
			HashAlgorithm algorithm = new SHA256Managed();

			byte[] plainTextWithSaltBytes = new byte[plainText.Length + salt.Length];

			for(int i = 0; i < plainText.Length; i++)
			{
				plainTextWithSaltBytes[i] = plainText[i];
			}
			for(int i = 0; i < salt.Length; i++)
			{
				plainTextWithSaltBytes[plainText.Length + i] = salt[i];
			}

			return algorithm.ComputeHash(plainTextWithSaltBytes);
		}

		//
		// GET: /Account/Edit/5

		public ActionResult Edit(int id)
		{
			return View();
		}

		//
		// POST: /Account/Edit/5

		[HttpPost]
		public ActionResult Edit(int id, FormCollection collection)
		{
			try
			{
				// TODO: Add update logic here

				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}

		//
		// GET: /Account/Delete/5

		public ActionResult Delete(int id)
		{
			return View();
		}

		//
		// POST: /Account/Delete/5

		[HttpPost]
		public ActionResult Delete(int id, FormCollection collection)
		{
			try
			{
				// TODO: Add delete logic here

				return RedirectToAction("Index");
			}
			catch
			{
				return View();
			}
		}
	}
}
