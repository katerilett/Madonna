using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataModel.Class;
using LocomotionEngines;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Data.Entity.Validation;

namespace LocomotionWebApp.App_Start
{
	public class DatabaseInit
	{
		public static void ResetIfDirty()
		{
			//ResetTestDatabase();
			try
			{
				using(var ctx = new DataModelContext())
				{
					// Perform a single operation that will likely never change, to trigger the
					// exception if the model has been altered.
					ctx.Users.First(u => true);
				}
			}
			catch(InvalidOperationException)
			{
				ResetTestDatabase();
			}
		}

		private static void ResetTestDatabase()
		{
			using(var c = new DataModelContext())
			{
				c.Database.Delete();

				// Test user.
				User testUser = new User()
					{
						Username = "admin",
						RealName = "Admin Team",

						// Password: test
						PasswordHash = new byte[]
						{117, 173, 203, 91, 75, 63, 147, 6, 200, 176, 45, 40, 104, 7, 114, 213,
						169, 217, 130, 162, 40, 108, 195, 15, 113, 69, 32, 134, 67, 226, 143, 200},

						PasswordSalt = new byte[]
						{24, 10, 184, 91, 24, 196, 93, 99, 150, 30, 131, 109, 16, 28, 181, 193},

						AccessGroup = DataModel.Enum.UserAccessGroup.Administrator,
						LastLogin = DateTime.Now,
						EmailAddress = "test@example.com",
					};
				c.Users.Add(testUser);

				//c.Networks.Add(testnet1);
				XDocument doc = XDocument.Load(HttpContext.Current.Server.MapPath(@"\App_Data\MexNewSampleInputs.xml"));				
				var xmlE = new XmlEngine();
				var xmlnetwork = xmlE.XmlFileToNetwork(doc);
				xmlnetwork.Name = "Example file network test";
				xmlnetwork.Author = testUser;
				xmlnetwork.LastEdit = DateTime.Now;
				c.Networks.Add(xmlnetwork);

				try
				{
					c.SaveChanges();
				}
				catch(DbEntityValidationException e)
				{
					foreach(var i in e.EntityValidationErrors)
					{
						Console.WriteLine(i.ValidationErrors);
					}

				}

			}
		}
	}
}