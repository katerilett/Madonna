using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.Class;

namespace LocomotionEngines
{
	public class Report
	{
		public Network ReportedNetwork { get; set; }

		public UnoptimizedSection UnoptimizedReport { get; set; }
		public OptimizedSection OptimizedReport { get; set; }
	}

	public class UnoptimizedSection
	{

	}

	public class OptimizedSection
	{
		/// <summary>
		/// Warning: this is a reference to the Entity Framework instance, and may not be attached
		/// to the database when this object is used.
		/// </summary>
		public Optimization RawOptimization { get; set; }

		public int TotalCost { get; set; }

		public Dictionary<Link, LinkCost> LinkCosts { get; set; }
	}

	public struct LinkCost
	{
		public double LocomotiveCost { get; set; }
		public double CarFlowCost { get; set; }
	}
}
