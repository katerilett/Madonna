using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Class
{
	public class Order : ICloneable
	{
		[Key]
		public long ID { get; set; }

		/// <summary>
		/// An orderID from the XML file. Not used as a key since it may be a duplicate.
		/// </summary>
		public string XMLOrderID { get; set; }

		[Required]
		public Node Origin { get; set; }
		[Required]
		public Node Destination { get; set; }

		public int Cars { get; set; }

		public double Revenue { get; set; }

		/// <summary>
		/// Note: does NOT clone ID, origin, or destination.
		/// </summary>
		public object Clone()
		{
			return new Order()
			{
				XMLOrderID = XMLOrderID,

				Cars = Cars,
				Revenue = Revenue
			};
		}
	}
}
