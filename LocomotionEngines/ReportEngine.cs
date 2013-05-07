using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.Class;

namespace LocomotionEngines
{
	public sealed class ReportEngine
	{
		// Implement as a singleton. (Should refactor other engines like this.)
		private static ReportEngine instance;
		public static ReportEngine getInstance()
		{
			if(instance == null)
				instance = new ReportEngine();
			return instance;
		}
		private ReportEngine() {}

		public Report GenerateReport(Network net)
		{
			Optimization optimal = net.OptimizationResult;

			Report report = new Report();
			report.ReportedNetwork = net;
			report.UnoptimizedReport = new UnoptimizedSection();

			if(optimal != null)
			{
				var optimizedReport = new OptimizedSection()
				{
					TotalCost = optimal.TotalCost,
					LinkCosts = new Dictionary<Link, LinkCost>(),
					RawOptimization = optimal
				};

				foreach(var olink in optimal.Links)
				{
					var link = olink.Link;
					LinkCost lcost = new LinkCost();

					lcost.CarFlowCost = olink.Flow * link.Distance * net.CarCostPerMile;

					lcost.LocomotiveCost = olink.CurrentTrains * link.Distance
						* (net.FuelCostPerMile + net.NonFuelCostPerMile);

					optimizedReport.LinkCosts.Add(link, lcost);
				}

				report.OptimizedReport = optimizedReport;
			}

			return report;
		}
	}
}
