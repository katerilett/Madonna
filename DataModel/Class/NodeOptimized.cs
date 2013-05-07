using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataModel.Class
{
	public class NodeOptimized : ICloneable
	{
		[Key]
		public long ID { get; set; }

		public virtual Node Node { get; set; }

		public int FlowIn { get; set; }
		public int FlowOut { get; set; }

		public double FlowInSubscription() { return Node.CarCapacity == 0 ? 0 : FlowIn / (double)Node.CarCapacity; }
		public double FlowOutSubscription() { return Node.CarCapacity == 0 ? 0 : FlowOut / (double)Node.CarCapacity; }

		public ExpansionParameters Expansion { get; set; }
		public int? ExpansionSuggested { get; set; }

		/// <summary>
		/// Note: does NOT copy the Node that this object optimizes.
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			return new NodeOptimized()
			{
				FlowIn = FlowIn,
				FlowOut = FlowOut,

				Expansion = (ExpansionParameters)Expansion.Clone(),
				ExpansionSuggested = ExpansionSuggested
			};
		}
	}
}
