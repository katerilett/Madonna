using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataModel.Class
{
	public class LinkOptimized : ICloneable
	{
		[Key]
		public long ID { get; set; }

		public virtual Link Link { get; set; }

		public int CurrentTrains { get; set; }

		public long Flow { get; set; }

		public double TrainSubscription()
		{
			if(Link.MaxTrains == 0)
				return 0;
			return CurrentTrains / (double)Link.MaxTrains;
		}

		public ExpansionParameters Expansion { get; set; }
		public int? ExpansionSuggested { get; set; }

		/// <summary>
		/// Note: does NOT clone the link that this object is optimizing.
		/// </summary>
		public object Clone()
		{
			return new LinkOptimized()
			{
				CurrentTrains = CurrentTrains,
				Flow = Flow,

				Expansion = (ExpansionParameters)Expansion.Clone(),
				ExpansionSuggested = ExpansionSuggested
			};
		}
	}
}
