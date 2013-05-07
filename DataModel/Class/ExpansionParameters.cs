using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Class
{
	public class ExpansionParameters : ICloneable
	{
		public int? CapacityExpansionMaxPossible { get; set; }
		public double? CapacityExpansionCostPerUnit { get; set; }

		public object Clone()
		{
			return new ExpansionParameters()
			{
				CapacityExpansionCostPerUnit = CapacityExpansionCostPerUnit,
				CapacityExpansionMaxPossible = CapacityExpansionMaxPossible
			};
		}
	}
}
