using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataModel.Class;

namespace LocomotionWebApp.Models.ViewModels
{
	public class NetworkViewModel
	{
		public long ID { get; set; }
		public ICollection<Node> Nodes { get; set; }
		public ICollection<Link> Links { get; set; }
		public int Revision { get; set; }
		public Node NewNode { get; set; }
		public Link NewLink { get; set; }
		public Order NewOrder { get; set; }
		public String Name { get; set; }
		public bool OutOfDate { get; set; }
		public bool Optimized { get; set; }
		public int MaxCarsPerTrain { get; set; }
		public double NonFuelCostPerMile { get; set; }
		public double FuelCostPerMile { get; set; }
		public double CarCostPerMile { get; set; }
		public bool IsOneDirectional { get; set; }
		public List<Node> sCodeList { get; set; }
	}
}