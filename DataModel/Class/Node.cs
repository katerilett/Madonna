using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.Enum;

namespace DataModel.Class
{
	[DebuggerDisplay("{StationCode}")]
	public class Node : ICloneable
	{
		[Key]
		public long ID { get; set; }

		public string StationCode { get; set; }

		public string Name { get; set; }

		public int CarCapacity { get; set; }		

		public Point Location { get; set; }

		public virtual ICollection<Link> InLinks { get; set; }
		public virtual ICollection<Link> OutLinks { get; set; }

		public virtual ICollection<Order> InOrders { get; set; }
		public virtual ICollection<Order> OutOrders { get; set; }

		/// <summary>
		/// Note: does NOT copy links and orders.
		/// </summary>
		public object Clone()
		{
			return new Node()
			{
				StationCode = StationCode,

				Name = Name,

				CarCapacity = CarCapacity,

				Location = Location,

				InLinks = new List<Link>(),
				OutLinks = new List<Link>(),

				InOrders = new List<Order>(),
				OutOrders = new List<Order>(),
			};
		}
	}
}
