using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Class
{
	public class Network : ICloneable
	{
		[Key]
		public long ID { get; set; }

		// Change tracking variables
		public string Name { get; set; }

		public virtual User Author { get; set; }

		public DateTime LastEdit { get; set; }

		public virtual Network Parent { get; set; }
		public int Revision { get; set; }

		// True network-related variables
		public int MaxCarsPerTrain { get; set; }

		public double NonFuelCostPerMile { get; set; }

		public double FuelCostPerMile { get; set; }

		public double CarCostPerMile { get; set; }

		public virtual ICollection<Node> Nodes { get; set; }
		public virtual ICollection<Link> Links { get; set; }
		public virtual ICollection<Order> Orders { get; set; }

		/// <summary>
		/// If an optimization result has been calculated, it is referenced here.
		/// </summary>
		public virtual Optimization OptimizationResult { get; set; }


		/// <summary>
		/// Clones this network, creating new nodes, links, orders, etc.
		/// Does NOT copy the ID
		/// </summary>
		public object Clone()
		{
			Network net = new Network()
			{
				Author = Author,
				MaxCarsPerTrain = MaxCarsPerTrain,
				NonFuelCostPerMile = NonFuelCostPerMile,
				FuelCostPerMile = FuelCostPerMile,
				CarCostPerMile = CarCostPerMile,
			};

			if(OptimizationResult != null)
			{
				net.OptimizationResult = (Optimization)OptimizationResult.Clone();
				net.OptimizationResult.OptimizedNetwork = net;
			}

			// Copying nodes a bit more complicated so do it outside primitive cloning.
			// Make a mapping for old nodes to new nodes, for easy translation on links and orders.
			var oldToNewNodes = new Dictionary<Node, Node>();
			var oldToNewLinks = new Dictionary<Link, Link>();

			net.Nodes = Nodes.Select(n =>
				{
					Node newN = (Node)n.Clone();
					oldToNewNodes.Add(n, newN);
					return newN;
				}).ToList();

			net.Links = Links.Select(l =>
				{
					Link newL = (Link)l.Clone();
					newL.From = oldToNewNodes[l.From];
					newL.To = oldToNewNodes[l.To];

					oldToNewNodes[l.From].OutLinks.Add(newL);
					oldToNewNodes[l.To].InLinks.Add(newL);

					oldToNewLinks.Add(l, newL);

					return newL;
				}).ToList();

			net.Orders = Orders.Select(l =>
				{
					Order newO = (Order)l.Clone();
					newO.Origin = oldToNewNodes[l.Origin];
					newO.Destination = oldToNewNodes[l.Destination];

					oldToNewNodes[l.Origin].OutOrders.Add(newO);
					oldToNewNodes[l.Destination].InOrders.Add(newO);

					return newO;
				}).ToList();

			// Copy over the cloned optimization if it exists.
			if(OptimizationResult != null)
			{
				net.OptimizationResult = (Optimization)OptimizationResult.Clone();
				net.OptimizationResult.Nodes = OptimizationResult.Nodes.Select(n => {
					NodeOptimized newNO = (NodeOptimized)n.Clone();
					newNO.Node = oldToNewNodes[n.Node];
					return newNO;
				}).ToList();
				net.OptimizationResult.Links = OptimizationResult.Links.Select(l => {
					LinkOptimized newLO = (LinkOptimized)l.Clone();
					newLO.Link = oldToNewLinks[l.Link];
					return newLO;
				}).ToList();
			}

			return net;
		}
	}
}
