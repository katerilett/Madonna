using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.Class;

namespace LocomotionEngines
{
	public class RawOptimizedState
	{
		public enum RawQuality
		{
			Solved,
			Infeasible,
			Unbounded,
			TimedOut,
		}
		public RawQuality Quality { get; set; }
		public Dictionary<Node, Dictionary<Link, int>> flowDecisions;
		public Dictionary<Link, int> locomotiveDecisions;
		public int totalCost;
		public int suggestionCapitalCost;

		public Network solvedNetwork;

		internal RawOptimizedState()
		{}

		public Optimization ToOptimization(bool suggestion = false)
		{
			string unoptimalReason;
			switch(Quality)
			{
				case RawQuality.Infeasible:
				case RawQuality.Unbounded:
					if(suggestion)
						unoptimalReason = "Suggestions could not be generated for this network, as"
							+ " the suggestion generation system  could not find an optimal solution.";
					else
						unoptimalReason = "This network is not solvable. Check orders for correctness."
							+ "<br /><br />If infrastructure improvements make the network solvable,"
							+ " the Suggestions tab below lists possibilities.";
					break;

				case RawQuality.TimedOut:
					if(suggestion)
						unoptimalReason = "Suggestions could not be generated for this network in "
							+ "less than the maximum of " + OptimizationEngine.TIMEOUT_MAX_SECONDS*2
							+ " seconds. ";
					else
						unoptimalReason = "This network could not be solved in less than the maximum of "
							+ OptimizationEngine.TIMEOUT_MAX_SECONDS*2
							+ " seconds. ";
					unoptimalReason += "<br /><br />Reduce the complexity of the network by deleting network "
						+ " elements or contact an administrator to allow longer timeouts.";
					break;
				default:
					unoptimalReason = null;
					break;
			}
			var o = new Optimization()
			{
				 OptimizedNetwork = solvedNetwork,
				 TotalCost = totalCost,
				 TotalRevenue = (int)solvedNetwork.Orders.Sum(or => or.Revenue * or.Cars),
				 Optimal = Quality == RawQuality.Solved,
				 UnoptimalMessage = unoptimalReason
			};

			bool setZero = Quality != RawQuality.Solved;

			o.Nodes = solvedNetwork.Nodes.Select(n => { var no = new NodeOptimized()
			{
				Node = n,

				FlowIn = setZero ? 0 : flowDecisions.Sum(
					v => v.Value.Sum(
						l => l.Key.To == n ? l.Value : 0
					)
				),

				FlowOut = setZero ? 0 : flowDecisions.Sum(
					v => v.Value.Sum(
						l => l.Key.From == n ? l.Value : 0
					)
				),
			};
			return no; }).ToList();

			o.Links = solvedNetwork.Links.Select(l => { var lo = new LinkOptimized()
			{
				Link = l,

				Flow = setZero ? 0 : flowDecisions.Sum(
					v => v.Value.Sum(
						l2 => l2.Key == l ? l2.Value : 0
					)
				),

				CurrentTrains = setZero ? 0 : locomotiveDecisions[l]
			};
			return lo; }).ToList();

			return o;
		}
	}
}
