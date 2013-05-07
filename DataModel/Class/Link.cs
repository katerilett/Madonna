using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Class
{
	[DebuggerDisplay("{From.StationCode} => {To.StationCode} :: {Distance}")]
	public class Link : ICloneable
	{
		[Key]
		public long ID { get; set; }

		[Required]
		public virtual Node From { get; set; }
		[Required]
		public virtual Node To { get; set; }

		public double Distance { get; set; }

		public int MaxTrains { get; set; }
		public double FuelAdjustment { get; set; }

		/// <summary>
		/// Note: does NOT clone from to nodes or ID!
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new Link()
			{
				Distance = Distance,

				MaxTrains = MaxTrains,
				FuelAdjustment = FuelAdjustment
			};
		}
	}
}
