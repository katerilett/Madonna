using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Class
{
	public class Optimization : ICloneable
	{
		[Key]
		public long ID { get; set; }

		public virtual Network OptimizedNetwork { get; set; }

		public virtual ICollection<NodeOptimized> Nodes { get; set; }
		public virtual ICollection<LinkOptimized> Links { get; set; }

		public int TotalCost { get; set; }
		public int TotalRevenue { get; set; }

		public int SuggestionTotalCost { get; set; }
		public int SuggestionTotalRevenue { get; set; }
		public int SuggestionCapitalCost { get; set; }
		public ExpansionParameters DefaultNodeExpansion { get; set; }
		public ExpansionParameters DefaultLinkExpansion { get; set; }

		public bool OutOfDate { get; set; }

		/// <summary>
		/// Indicates if this is the optimal solution. (Vs. infeasible or unbounded.)
		/// </summary>
		public bool Optimal { get; set; }
		public string UnoptimalMessage { get; set; }

		public bool SuggestionOptimal { get; set; }
		public string SuggestionUnoptimalMessage { get; set; }

		/// <summary>
		/// Note: does not clone Nodes and Links lists, or Network reference.
		/// </summary>
		public object Clone()
		{
			return new Optimization()
			{
				TotalCost = TotalCost,
				SuggestionTotalCost = SuggestionTotalCost,
				SuggestionCapitalCost = SuggestionCapitalCost,

				DefaultNodeExpansion = (ExpansionParameters)DefaultNodeExpansion.Clone(),
				DefaultLinkExpansion = (ExpansionParameters)DefaultLinkExpansion.Clone(),

				OutOfDate = OutOfDate,
				Optimal = Optimal
			};
		}
	}
}
