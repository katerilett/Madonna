using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataModel.Class;
using LocomotionEngines;
using LocomotionWebApp.Models.ViewModels;

namespace LocomotionWebApp.Controllers
{
	public class ReportController : Controller
	{
		[Authorize]
		public ActionResult View(long id, bool createIfNotFound = false)
		{
			var rvm = new ReportViewModel();
			rvm.ID = id;

			using(var c = new DataModelContext())
			{
				Network net = c.Networks
					.Include("Nodes")
					.Include("Links")
					.Include("Orders")
					.Include("OptimizationResult.Nodes")
					.Include("OptimizationResult.Links.Link")
					.Include("OptimizationResult.Links.Link.To")
					.Include("OptimizationResult.Links.Link.From")
					.SingleOrDefault(n => n.ID == id); 

				if(net.OptimizationResult == null && createIfNotFound)
				{
					net.Optimize();
					c.SaveChanges();
				}
				rvm.Report = ReportEngine.getInstance().GenerateReport(net);
			}

			return View(rvm);
		}
	}
}