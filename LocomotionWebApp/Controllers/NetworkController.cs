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

namespace LocomotionWebApp.Controllers
{
	public class NetworkController : Controller
	{
		//test
		// GET: /Network/
		[Authorize]
		public ActionResult Index()
		{
			var nvm = new NetworkListViewModel();

			using(var c = new DataModelContext())
			{
				nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();
			}

			object alertObject;
			if(TempData.TryGetValue("Alert", out alertObject))
			{
				ViewBag.Alert = alertObject;
			}
			
			return View(nvm);
		}

		[Authorize]
		public ActionResult View(long id)
		{
			var nvm = new NetworkViewModel();

			using(var c = new DataModelContext())
			{
				var network = c.Networks.Find(id);
				if(network == null)
				{
					TempData["Alert"] = "The selected network does not exist.";
					return RedirectToAction("Index");
				}
				
				List<Node> tempList = (from n in network.Nodes
									   where n.StationCode != null
									   select n).ToList();
					
								

				nvm.ID = network.ID;
				nvm.Nodes = network.Nodes;
				nvm.Links = network.Links;
				nvm.sCodeList = tempList;
				nvm.Revision = network.Revision;
				

				var nameNet = network;
				while(nameNet.Parent != null && nameNet.Name == null)
					nameNet = nameNet.Parent;
				nvm.Name = nameNet.Name;

				if(network.OptimizationResult == null)
				{
					nvm.OutOfDate = false;
					nvm.Optimized = false;
				}
				else
				{
					nvm.OutOfDate = network.OptimizationResult.OutOfDate;
					nvm.Optimized = true;
				}
			}

			return View("View", nvm);
		}

		[Authorize]
		public ActionResult Delete(long id)
		{
			using(var c = new DataModelContext())
			{
				var network = c.Networks.Find(id);
				network.Name = null;
				c.SaveChanges();
			}
			return RedirectToAction("Index");
		}

		[Authorize]
		public ActionResult NetworkHistory(long id)
		{
			var nvm = new NetworkListViewModel();
			nvm.Networks = new List<Network>();

			using(var c = new DataModelContext())
			{
				var network = c.Networks
					.Include("Author")
					.Include("Parent")
					.SingleOrDefault(n => n.ID == id);
				while(true)
				{
					nvm.Networks.Add(network);
					if (network.Parent == null)
						break;
					network = c.Networks.Include("Author").Include("Parent")
						.SingleOrDefault(n => n.ID == network.Parent.ID);
				}
			}

			return View(nvm);
		}

		[Authorize]
		public ActionResult TempEdit(long id, int clickTab)
		{
			Network newNetwork;
			int tab = 0;
			tab = clickTab;
			using(var c = new DataModelContext())
			{
				var originalNetwork = c.Networks.Find(id);

				newNetwork = (Network)originalNetwork.Clone();
				newNetwork.Author = UserDataEngine.getInstance().GetCurrentUser(c, HttpContext);
				newNetwork.LastEdit = DateTime.Now;
				newNetwork.Parent = originalNetwork;
				newNetwork.Revision = originalNetwork.Revision + 1;

				c.Networks.Add(newNetwork);
				c.SaveChanges();
			}

			ViewBag.ID = newNetwork.ID;

			return RedirectToAction("View", new { id = newNetwork.ID, tab = tab });
		}

		[Authorize]
		public ActionResult OptimizationSidebar(long id, int startTab = 1)
		{
			var osvm = new OptimizationSidebarViewModel();
			osvm.StartTab = startTab;
			using(var c = new DataModelContext())
			{
				var network = c.Networks
					.Include("OptimizationResult")
					.Include("OptimizationResult.Nodes")
					.Include("OptimizationResult.Nodes.Node")
					.Include("OptimizationResult.Links")
					.Include("OptimizationResult.Links.Link")
					.FirstOrDefault(n => n.ID == id);
				osvm.Optimization = network.OptimizationResult;
			}
			return View(osvm);
		}

		[Authorize]
		public JsonResult GetNetwork(long id)
		{
			using(var c = new DataModelContext())
			{
				Network net = c.Networks.Find(id);
				Optimization opt = net.OptimizationResult;


				if(net == null)
				{
					return Json(new {
							failure=true, failureMessage="The selected network does not exist."
						},
						JsonRequestBehavior.AllowGet);
				}

				if(opt == null)
				{
					return Json(new
					{		
				MaxCarsPerTrain = net.MaxCarsPerTrain,
				NonFuelCostPerMile = net.NonFuelCostPerMile,
				FuelCostPerMile = net.FuelCostPerMile,
				CarCostPerMile = net.CarCostPerMile,

						nodes = net.Nodes.ToDictionary(
							n => n.ID.ToString(),
							n => new
							{
							id = n.ID,
								name = n.Name,
								stationcode = n.StationCode,
								longitude = n.Location.Longitude,
								latitude = n.Location.Latitude,
								carCapacity = n.CarCapacity,
							}
						),
					nodesArray = net.Nodes.Select(n => new
					{
						id = n.ID,
						name = n.Name,
						stationcode = n.StationCode,
						longitude = n.Location.Longitude,
						latitude = n.Location.Latitude,
						carCapacity = n.CarCapacity,
					}),
						links = net.Links.Select(l => new {
							id = l.ID,
							fromid = l.From.ID,
							toid = l.To.ID,
							distance = l.Distance,
							maxTrains = l.MaxTrains,
							fuelAdjustment = l.FuelAdjustment
						}),

						orders = net.Orders.Select(l => new
						{
							id = l.ID,
							xmlID = l.XMLOrderID,
							fromid = l.Origin.ID,
							toid = l.Destination.ID,
							cars = l.Cars,
							revenue = l.Revenue
						})

					}, JsonRequestBehavior.AllowGet);
				}

				return Json(new
				{
					MaxCarsPerTrain = net.MaxCarsPerTrain,
					NonFuelCostPerMile = net.NonFuelCostPerMile,
					FuelCostPerMile = net.FuelCostPerMile,
					CarCostPerMile = net.CarCostPerMile,

					nodes = net.Nodes.ToDictionary(
						n => n.ID.ToString(),
						n => new
						{   id = n.ID,
							name = n.Name,
							stationcode = n.StationCode,
							longitude = n.Location.Longitude,
							latitude = n.Location.Latitude,
							carCapacity = n.CarCapacity,
						}
					),
					nodesArray = net.Nodes.Select(n => new
					{   name = n.Name,
						id = n.ID,
						stationcode = n.StationCode,
						longitude = n.Location.Longitude,
						latitude = n.Location.Latitude,
						carCapacity = n.CarCapacity,
					}),
					links = net.Links.Select(l => new {
						id = l.ID,
						fromid = l.From.ID,
						toid = l.To.ID,
						distance = l.Distance,
						maxTrains = l.MaxTrains,
						fuelAdjustment = l.FuelAdjustment
					}),

					orders = net.Orders.Select(l => new
					{
						id = l.ID,
						xmlID = l.XMLOrderID,
						fromid = l.Origin.ID,
						toid = l.Destination.ID,
						cars = l.Cars,
						revenue = l.Revenue
					}),


					optimizedNodes = opt.Nodes.ToDictionary(
						n => n.Node.ID.ToString(),
						n => new
						{
							inFlow = n.FlowIn,
							outFlow = n.FlowOut,
						}
					),
					optimizedLinks = opt.Links.ToDictionary(
						l => l.Link.ID.ToString(),
						l => new
						{
							currentTrains = l.CurrentTrains,
							currentFlow = l.Flow,
							supscription = l.TrainSubscription()
						}
					)




				}, JsonRequestBehavior.AllowGet);
			}
		}

		[Authorize]
		public JsonResult GetOptimization(long id, bool createOptimization = false)
		{
			using(var c = new DataModelContext())
			{
				Network net = c.Networks.Find(id);
				Optimization opt = net.OptimizationResult;

				if(createOptimization)
				{
					net.Optimize();
					opt = net.OptimizationResult;
					opt.OutOfDate = false;
					c.SaveChanges();
				}
				else if(opt == null)
				{
					return Json(
						new {
							failure=true,
							failureMessage="The selected network is not optimized."
						},
						JsonRequestBehavior.AllowGet);
				}

				return Json(new
				{
					nodes = opt.Nodes.ToDictionary(
						n => n.Node.ID.ToString(),
						n => new
						{
							inFlow = n.FlowIn,
							outFlow = n.FlowOut,
						}
					),
					links = opt.Links.ToDictionary(
						l => l.Link.ID.ToString(),
						l => new
						{
							currentTrains = l.CurrentTrains,
							currentFlow = l.Flow,
							subscription = l.TrainSubscription()
						}
					),
					orders = net.Orders.Select(l => new
					{
						id = l.ID,
						xmlID = l.XMLOrderID,
						fromid = l.Origin.ID,
						toid = l.Destination.ID,
						cars = l.Cars,
						revenue = l.Revenue
					})
				}, JsonRequestBehavior.AllowGet);
			}
		}

		[Authorize]
		public ActionResult CreateBlank(string NetworkName)
		{
			var nvm = new NetworkListViewModel();

			if(NetworkName == "")
			{
				ViewBag.UploadAlert = "Enter a network name";

				using (var c = new DataModelContext())
				{
					nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();
				}
				return View("Index", nvm);
			}

			using(var c = new DataModelContext())
			{
				var xmlnetwork = new Network();

				xmlnetwork.Name = NetworkName;
				xmlnetwork.Author = UserDataEngine.getInstance().GetCurrentUser(c, HttpContext);
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
					throw e;
				}
				nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();

				ViewBag.NewNetworkID = xmlnetwork.ID;
			}

			ViewBag.Alert = "Network upload successful";
			ViewBag.AlertClass = "alert-success";
			return View("Index", nvm);
		}

		[Authorize]
		public ActionResult Upload(string NetworkName, IEnumerable<HttpPostedFileBase> files)
		{
			var nvm = new NetworkListViewModel();

			IEnumerable<HttpPostedFileBase> someFiles = files;

			var networkDoc = new XDocument();			

			if(NetworkName == "")
			{
				ViewBag.UploadAlert = "Enter a network name";

				using (var c = new DataModelContext())
				{
					nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();
				}
				return View("Index", nvm);
			}

			try
			{
				networkDoc = XDocument.Load(Request.Files["NetworkFile"].InputStream);				
			}
			catch (XmlException e)
			{
				Console.WriteLine(e.Message);
				ViewBag.UploadAlert = "You must select a valid xml file";

				using (var c = new DataModelContext())
				{
					nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();
				}
				return View("Index", nvm);
			}

			using(var c = new DataModelContext())
			{

				var xmlE = new XmlEngine();
				var xmlnetwork = xmlE.XmlFileToNetwork(networkDoc);

				xmlnetwork.Name = NetworkName;
				xmlnetwork.Author = UserDataEngine.getInstance().GetCurrentUser(c, HttpContext);
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
					throw e;
				}
				nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();

				ViewBag.NewNetworkID = xmlnetwork.ID;
			}

			ViewBag.Alert = "Network upload successful";
			ViewBag.AlertClass = "alert-success";
			return View("Index", nvm);
		}

		[Authorize]
		public ActionResult UploadUrl(string NetworkName, string NetworkUrl)
		{
			var nvm = new NetworkListViewModel();

			Network net;

			if(NetworkName == "")
			{
				ViewBag.UrlUploadAlert = "Enter a network name";

				using (var c = new DataModelContext())
				{
					nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();
				}
				return View("Index", nvm);
			}

			try
			{
				net = new XmlEngine().UrlDownloadToNetwork(new Uri(NetworkUrl));
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				ViewBag.UrlUploadAlert = "Could not find valid XML file at specified URL";

				using (var c = new DataModelContext())
				{
					nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();
				}
				return View("Index", nvm);
			}

			using(var c = new DataModelContext())
			{
				net.Name = NetworkName;
				net.Author = UserDataEngine.getInstance().GetCurrentUser(c, HttpContext);
				net.LastEdit = DateTime.Now;
				c.Networks.Add(net);

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
					throw e;
				}
				nvm.Networks = c.Networks.Include("Author").Where(n => n.Name != null).ToList();

				ViewBag.NewNetworkID = net.ID;
			}

			ViewBag.Alert = "Network upload successful";
			ViewBag.AlertClass = "alert-success";
			return View("Index", nvm);
		}

		[Authorize]
		public ActionResult SaveNetwork(NetworkViewModel model)
		{
			using (var c = new DataModelContext())
			{
				var net = c.Networks.Find(model.ID);
				net.LastEdit = DateTime.Now;

				net.Name = net.Parent.Name;
				net.Parent.Name = null;

				c.SaveChanges();
			}
			return TempEdit(model.ID, 0);
		}

		[Authorize]
		public ActionResult SaveNetworkAs(NetworkViewModel model)
		{
			using (var c = new DataModelContext())
			{
				var net = c.Networks.Find(model.ID);
				net.Name = model.Name;
				net.LastEdit = DateTime.Now;
				c.SaveChanges();
			}
			return TempEdit(model.ID, 0);
		}

		[Authorize]
		public ActionResult editNetwork(NetworkViewModel model)
		{
			killOptimization(model.ID);
			using (var c = new DataModelContext())
			{
				var net = c.Networks.Find(model.ID);
			
				net.MaxCarsPerTrain = model.MaxCarsPerTrain;
				net.NonFuelCostPerMile = model.NonFuelCostPerMile;
				net.FuelCostPerMile = model.FuelCostPerMile;
				net.CarCostPerMile = model.CarCostPerMile;
				net.OptimizationResult = null;

			
				net.LastEdit = DateTime.Now;
				c.SaveChanges();
				
			}
			return Json(new { success = true });
		}

		[Authorize]
		public void killOptimization(long id)
		{
			
			
			using (var c = new DataModelContext())
			{
				var net = c.Networks
					.Include("OptimizationResult")
					.Include("OptimizationResult.Nodes")
					.Include("OptimizationResult.Links")
					.Include("OptimizationResult.OptimizedNetwork")
					.FirstOrDefault(n => n.ID == id);

				if (net.OptimizationResult == null)
				{
				}
				else{
				var opt = net.OptimizationResult;
				net.OptimizationResult.OptimizedNetwork = null;
				net.OptimizationResult = null;

				net.LastEdit = DateTime.Now;
				c.SaveChanges();
				}
			}
			
		}


		[Authorize]
		public JsonResult addNode(NetworkViewModel model)
		{
			Node newNode = model.NewNode;

			killOptimization(model.ID);

			using(var c = new DataModelContext())
			{
				var net = c.Networks.Find(model.ID);
				net.Nodes.Add(newNode);							
				net.LastEdit = DateTime.Now;
				c.SaveChanges();
			}
			return Json(new { success=true });
		}

		[Authorize]
		public JsonResult editNode(NetworkViewModel model)
		{
			ICollection<Node> nodeArray = new List<Node>();
			killOptimization(model.ID);

			using (var c = new DataModelContext())
			{
				foreach (Node n in c.Networks.Find(model.ID).Nodes)
				{
					if (n.StationCode == model.NewNode.StationCode)
					{
						n.Name = model.NewNode.Name;
						n.CarCapacity = model.NewNode.CarCapacity;
						n.Location = model.NewNode.Location;
						
					}
				}

				var net = c.Networks.Find(model.ID);
				net.LastEdit = DateTime.Now;

				c.SaveChanges();
			}
			return Json(new { success = true });
		}

		[Authorize]
		public JsonResult removeNode(NetworkViewModel model)
		{
			ICollection<Node> nodeArray = new List<Node>();
			ICollection<Link> linkArray = new List<Link>();
			ICollection<Order> orderArray = new List<Order>();
			killOptimization(model.ID);

			using(var c = new DataModelContext())
			{
				foreach(Node n in c.Networks.Find(model.ID).Nodes)
				{
					if(n.StationCode == model.NewNode.StationCode)
					{
						nodeArray.Add(n);
					}
				}
				foreach(Link l in c.Networks.Find(model.ID).Links)
				{

					if(l.From.StationCode == model.NewNode.StationCode || l.To.StationCode == model.NewNode.StationCode)
					{
						linkArray.Add(l);
					}

				}
				foreach(Order o in c.Networks.Find(model.ID).Orders)
				{


					if(o.Origin.StationCode == model.NewNode.StationCode || o.Destination.StationCode == model.NewNode.StationCode)
					{

						orderArray.Add(o);
					}
				}

				var net = c.Networks.Find(model.ID);
				foreach(Node n in nodeArray)
				{
					net.Nodes.Remove(n);
				}

				foreach(Link l in linkArray)
				{
					net.Links.Remove(l);
				}

				foreach(Order o in orderArray)
				{
					net.Orders.Remove(o);
				}
				net.LastEdit = DateTime.Now;

				c.SaveChanges();
			}
			return Json(new { success=true });
		}

		[Authorize]
		public JsonResult addOrder(NetworkViewModel model)
		{

			Order newOrder = model.NewOrder;

			Node to = new Node();
			Node from = new Node();
			killOptimization(model.ID);

			using(var c = new DataModelContext())
			{
				foreach(Node n in c.Networks.Find(model.ID).Nodes)
				{

					if(n.StationCode == model.NewOrder.Destination.StationCode)
					{

						to = n;
					}
					else if(n.StationCode == model.NewOrder.Origin.StationCode)
					{
						from = n;
					}
				}
				newOrder.Destination = to;
				newOrder.Origin = from;

				var net = c.Networks.Find(model.ID);
				net.Orders.Add(newOrder);
			
				net.LastEdit = DateTime.Now;
				c.SaveChanges();
			}
			return Json(new { success=true });
		}

		[Authorize]
		public JsonResult removeOrder(NetworkViewModel model)
		{

			ICollection<Order> orderArray = new List<Order>();
			killOptimization(model.ID);

			using(var c = new DataModelContext())
			{
				var net = c.Networks
					.Include("Orders.Origin")
					.Include("Orders.Destination")
					.FirstOrDefault(n => n.ID == model.ID);
				
				foreach(Order o in net.Orders)
				{
					Order whyistherenoorder = o;
					if(o.Origin.StationCode == model.NewOrder.Origin.StationCode && o.Destination.StationCode == model.NewOrder.Destination.StationCode)
					{
						orderArray.Add(o);
					}
				}

				foreach(Order o in orderArray)
				{
					net.Orders.Remove(o);
				}	
				net.LastEdit = DateTime.Now;
				c.SaveChanges();
			}
			return Json(new { success=true });
		}
		
				
						
		[Authorize]
		public JsonResult addLink(NetworkViewModel model)
		{

			Link newLink = model.NewLink;
			Link newLink2 = model.NewLink;

			Node to = new Node();
			Node from = new Node();

			Node to2 = new Node();
			Node from2 = new Node();
			killOptimization(model.ID);

			using(var c = new DataModelContext())
			{
				foreach(Node n in c.Networks.Find(model.ID).Nodes)
				{

					if(n.StationCode == model.NewLink.To.StationCode)
					{

						to = n;
					}
					else if(n.StationCode == model.NewLink.From.StationCode)
					{
						from = n;
					}
				}

				newLink.From = from;
				newLink.To = to;

				try
				{
					var net = c.Networks.Find(model.ID);
					net.Links.Add(newLink);
					net.LastEdit = DateTime.Now;
				}
				catch(Exception e)
				{
					throw e;
				}
				c.SaveChanges();
			}

			if (!model.IsOneDirectional)
			{
				using (var d = new DataModelContext())
				{

					foreach (Node n in d.Networks.Find(model.ID).Nodes)
					{

						if (n.StationCode == model.NewLink.To.StationCode)
						{

							to2 = n;
						}
						else if (n.StationCode == model.NewLink.From.StationCode)
						{
							from2 = n;
						}
					}
					newLink2.From = to2;
					newLink2.To = from2;
					try
					{
						var net = d.Networks.Find(model.ID);
						net.Links.Add(newLink2);
						net.LastEdit = DateTime.Now;
					}
					catch (Exception e)
					{
						throw e;
					}
					d.SaveChanges();
				}
			}
			return Json(new { success=true });
		}

		[Authorize]
		public JsonResult editLink(NetworkViewModel model)
		{
			ICollection<Link> linkArray = new List<Link>();
			killOptimization(model.ID);

			using (var c = new DataModelContext())
			{
				foreach (Link n in c.Networks.Find(model.ID).Links)
				{
					if (n.ID == model.NewLink.ID)
					{
						foreach (Node node in c.Networks.Find(model.ID).Nodes)
						{
							if (node.StationCode == model.NewLink.From.StationCode)
							{

								n.From = node;
							}
							else if (node.StationCode == model.NewLink.To.StationCode)
							{
								n.To = node;
							}
						}
						n.FuelAdjustment = model.NewLink.FuelAdjustment;
						n.Distance = model.NewLink.Distance;
						n.MaxTrains = model.NewLink.MaxTrains;
					}
				}
				

				var net = c.Networks.Find(model.ID);
					
				net.LastEdit = DateTime.Now;

				c.SaveChanges();
			}
			return Json(new { success = true });
		}

		[Authorize]
		public JsonResult removeLink(NetworkViewModel model)
		{
			killOptimization(model.ID);
			ICollection<Link> linkArray = new List<Link>();

			using(var c = new DataModelContext())
			{
				var net = c.Networks.Find(model.ID);

				foreach(Link l in net.Links)
				{

					if(l.From.StationCode == model.NewLink.From.StationCode &&
						l.To.StationCode == model.NewLink.To.StationCode)
					{
						linkArray.Add(l);
					}

					if (!model.IsOneDirectional)
					{
						if (l.To.StationCode == model.NewLink.From.StationCode &&
							l.From.StationCode == model.NewLink.To.StationCode)
						{
							linkArray.Add(l);
						}
					}

				}
			
				foreach(Link l in linkArray)
				{
					net.Links.Remove(l);
				}
							
				net.LastEdit = DateTime.Now;
				c.SaveChanges();
			}
			return Json(new { success=true });
		}


		[Authorize]
		public JsonResult editOrder(NetworkViewModel model)
		{
			ICollection<Order> orderArray = new List<Order>();
			killOptimization(model.ID);

			using (var c = new DataModelContext())
			{
				foreach (Order n in c.Networks.Find(model.ID).Orders)
				{
					if (n.ID == model.NewOrder.ID)
					{
						foreach (Node node in c.Networks.Find(model.ID).Nodes)
						{
							if (node.StationCode == model.NewOrder.Origin.StationCode)
							{
								n.Origin = node;
							}
							else if (node.StationCode == model.NewOrder.Destination.StationCode)
							{
								n.Destination = node;
							}
						}
						n.Revenue = model.NewOrder.Revenue;
						n.Cars = model.NewOrder.Cars;
					}
				}
				
				var net = c.Networks.Find(model.ID);
						
				net.LastEdit = DateTime.Now;

				c.SaveChanges();
			}
			return Json(new { success = true });
		}

		[Authorize]
		public JsonResult GetSuggestionData(int id)
		{
			var nodes = new Dictionary<string, object>();
			var links = new Dictionary<string, object>();


			using(var c = new DataModelContext())
			{
				var opt = c.Optimizations
					.Include("Nodes")
					.Include("Links")
					.FirstOrDefault(o => o.ID == id);

				foreach(var node in opt.Nodes)
				{
					nodes.Add(node.ID.ToString(), new {
						id = node.ID,
						onode = node.Clone(),
						code = node.Node.StationCode
					});
				}
				foreach(var link in opt.Links)
				{
					links.Add(link.ID.ToString(), new {
						id = link.ID,
						olink = link.Clone(),
						from = link.Link.From.StationCode,
						to = link.Link.To.StationCode,
					});
				}
			}
			return Json(new { nodes=nodes, links=links }, JsonRequestBehavior.AllowGet);
		}

		[Authorize]
		public JsonResult SetExpansionData(int id, int? maxExp, int? costPerCar, bool node)
		{
			using(var c = new DataModelContext())
			{
				Optimization opt;

				ExpansionParameters p;
				if(node)
				{
					opt = c.Optimizations
						.Include("OptimizedNetwork")
						.FirstOrDefault(o => o.Nodes.Count(n => n.ID == id) > 0);
					p = c.NodesOptimized.Find(id).Expansion;
				}
				else
				{
					opt = c.Optimizations
						.Include("OptimizedNetwork")
						.FirstOrDefault(o => o.Links.Count(n => n.ID == id) > 0);
					p = c.LinksOptimized.Find(id).Expansion;
				}
				opt.OutOfDate = true;

				// If maxExp is -1, set to null. Otherwise, set to the given value.
				if(maxExp != null)
					p.CapacityExpansionMaxPossible = maxExp == -1 ? null : maxExp;
				if(costPerCar != null)
					p.CapacityExpansionCostPerUnit = costPerCar == -1 ? null : costPerCar;

				c.SaveChanges();
			}
			return Json(new { success=true }, JsonRequestBehavior.AllowGet);
		}

		[Authorize]
		public JsonResult ImplementSuggestion(int oid, int id, bool node)
		{
			using(var c = new DataModelContext())
			{
				Optimization opt = c.Optimizations
						.Include("OptimizedNetwork")
						.Include("OptimizedNetwork.Links")
						.Include("OptimizedNetwork.Links.To")
						.Include("OptimizedNetwork.Links.From")
						.Include("OptimizedNetwork.Nodes")
						.FirstOrDefault(o => o.ID == oid);

				if(node)
				{
					var nodeObj = opt.Nodes.FirstOrDefault(n => n.ID == id);
					// Assume suggestion exists.
					nodeObj.Node.CarCapacity += (int)(nodeObj.ExpansionSuggested);
				}
				else
				{
					var linkObj = opt.Links.FirstOrDefault(n => n.ID == id);
					// Assume suggestion exists.
					linkObj.Link.MaxTrains += (int)(linkObj.ExpansionSuggested);
				}
				opt.OutOfDate = true;

				try
				{
					c.SaveChanges();
				}
				catch(DbEntityValidationException e)
				{
					var eve = e.EntityValidationErrors;
					throw e;
				}
			}
			return Json(new { success=true }, JsonRequestBehavior.AllowGet);
		}

		[Authorize]
		public JsonResult ImplementAllSuggestions(int oid)
		{
			using(var c = new DataModelContext())
			{
				Optimization opt = c.Optimizations
						.Include("OptimizedNetwork")
						.Include("OptimizedNetwork.Links")
						.Include("OptimizedNetwork.Links.To")
						.Include("OptimizedNetwork.Links.From")
						.Include("OptimizedNetwork.Nodes")
						.FirstOrDefault(o => o.ID == oid);

				foreach(var n in opt.Nodes)
				{
					n.Node.CarCapacity += (int)(n.ExpansionSuggested);
				}
				foreach(var l in opt.Links)
				{
					l.Link.MaxTrains += (int)(l.ExpansionSuggested);
				}
				opt.OutOfDate = true;

				try
				{
					c.SaveChanges();
				}
				catch(DbEntityValidationException e)
				{
					var eve = e.EntityValidationErrors;
					throw e;
				}
			}
			return Json(new { success=true }, JsonRequestBehavior.AllowGet);
		}

		[Authorize]
		public JsonResult SetDefaultExpansionData(
			int id,
			int? maxNodeExp,
			int? nodeCostPer,
			int? maxLinkExp,
			int? linkCostPer)
		{
			using(var c = new DataModelContext())
			{
				var opt = c.Optimizations
					.Include("OptimizedNetwork")
					.FirstOrDefault(o => o.ID == id);

				if(maxNodeExp != null)
					opt.DefaultNodeExpansion.CapacityExpansionMaxPossible = maxNodeExp;
				if(nodeCostPer != null)
					opt.DefaultNodeExpansion.CapacityExpansionCostPerUnit = nodeCostPer;
				if(maxLinkExp != null)
					opt.DefaultLinkExpansion.CapacityExpansionMaxPossible = maxLinkExp;
				if(linkCostPer != null)
					opt.DefaultLinkExpansion.CapacityExpansionCostPerUnit = linkCostPer;

				opt.OutOfDate = true;

				try
				{
					c.SaveChanges();
				}
				catch(DbEntityValidationException e)
				{
					var eve = e.EntityValidationErrors;
					throw e;
				}
			}
			return Json(new { success=true }, JsonRequestBehavior.AllowGet);
		}
	}
}
