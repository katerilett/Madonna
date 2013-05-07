using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.Class;
using Microsoft.SolverFoundation.Solvers;
using Microsoft.SolverFoundation.Services;
using Microsoft.SolverFoundation.Common;
using SolverFoundation.Plugin.Gurobi;
using System.Threading;

namespace LocomotionEngines
{
	public static class OptimizationExtensions
	{
		public static void Optimize(this Network net)
		{
			RawOptimizedState optWithoutExpansion = OptimizationEngine.getInstance().Optimize(net,
				new OptimizationEngine.OptimizationOptions() { });

			net.SetFrom(optWithoutExpansion.ToOptimization());

			RawOptimizedState optWithExpansion = OptimizationEngine.getInstance().Optimize(net,
				new OptimizationEngine.OptimizationOptions() {
					AllowLocomotiveCapacityExpansion = true,
					AllowNodeCapacityExpansion = true,
				});

			var optWithExp = optWithExpansion.ToOptimization(true);

			net.OptimizationResult.SuggestionTotalCost = optWithExpansion.totalCost;
			net.OptimizationResult.SuggestionTotalRevenue = optWithExp.TotalRevenue;
			net.OptimizationResult.SuggestionCapitalCost = optWithExpansion.suggestionCapitalCost;

			net.OptimizationResult.SuggestionOptimal =
				optWithExpansion.Quality == RawOptimizedState.RawQuality.Solved;
			net.OptimizationResult.SuggestionUnoptimalMessage = optWithExp.UnoptimalMessage;
		}

		public static void SetFrom(this Network net, Optimization oOpt)
		{
			if(net.OptimizationResult == null)
				net.OptimizationResult = oOpt;
			else
			{
				var nOpt = net.OptimizationResult;
				foreach(var node in nOpt.Nodes)
				{
					var oNode = oOpt.Nodes.FirstOrDefault(no => no.Node == node.Node);

					node.FlowIn = oNode.FlowIn;

					node.FlowOut = oNode.FlowOut;
				}
				foreach(var link in nOpt.Links)
				{
					var oLink = oOpt.Links.FirstOrDefault(lo => lo.Link == link.Link);

					link.Flow = oLink.Flow;
					link.CurrentTrains = oLink.CurrentTrains;
				}
				nOpt.Optimal = oOpt.Optimal;
				nOpt.UnoptimalMessage = oOpt.UnoptimalMessage;
				nOpt.TotalCost = oOpt.TotalCost;
				nOpt.TotalRevenue = oOpt.TotalRevenue;
			}
		}
	}

	public sealed class OptimizationEngine
	{
		public static int TIMEOUT_MAX_SECONDS = 20;
		private static OptimizationEngine instance;
		public static OptimizationEngine getInstance()
		{
			if(instance == null)
				instance = new OptimizationEngine();
			return instance;
		}
		private OptimizationEngine() {}

		public struct OptimizationOptions
		{
			// These two fields (for now) require that the network already have been optimized.
			public bool AllowLocomotiveCapacityExpansion { get; set; }
			public bool AllowNodeCapacityExpansion { get; set; }
		}

		public RawOptimizedState Optimize(
			Network net,
			OptimizationOptions options)
		{
			var context = SolverContext.GetContext();
			context.ClearModel();
			var model = context.CreateModel();

			// Load in nodes, links, orders as lists.
			var nodes = net.Nodes.ToList();
			var links = net.Links.ToList();
			var orders = net.Orders.ToList();

			// Make sure the existing optimization has default weights set.
			if(options.AllowLocomotiveCapacityExpansion)
			{
				if(net.OptimizationResult.DefaultLinkExpansion == null)
				{
					net.OptimizationResult.DefaultLinkExpansion = new ExpansionParameters()
					{
						CapacityExpansionMaxPossible = 100,
						CapacityExpansionCostPerUnit = 10000
					};
				}
			}
			if(options.AllowNodeCapacityExpansion)
			{
				if(net.OptimizationResult.DefaultNodeExpansion == null)
				{
					net.OptimizationResult.DefaultNodeExpansion = new ExpansionParameters()
					{
						CapacityExpansionMaxPossible = 10000,
						CapacityExpansionCostPerUnit = 1000
					};
				}
			}

			#region Decision variables, car costs
			var flowDecisions = new Dictionary<Node,Dictionary<Link,Decision>>();
			Term totalCarCosts = 0;

			var expandNodeDecisions = new Dictionary<NodeOptimized,Decision>();
			var expandLinkDecisions = new Dictionary<LinkOptimized,Decision>();
			Term totalExpandCosts = 0;

			// One per link for each node.
			foreach(var node in nodes)
			{
				flowDecisions[node] = new Dictionary<Link,Decision>();
				foreach(var link in links)
				{
					flowDecisions[node][link] = new Decision(
						Domain.IntegerNonnegative,
						"Node_" + node.ID + "_cars_" +
						link.From.ID + "_to_" + link.To.ID
					);

					model.AddDecision(flowDecisions[node][link]);

					// Car cost for this link.
					totalCarCosts += flowDecisions[node][link] * link.Distance * net.CarCostPerMile;
				}
			}
			// Create the decision vars for expansion, if necessary.
			if(options.AllowNodeCapacityExpansion)
			{
				foreach(var node in net.OptimizationResult.Nodes)
				{
					expandNodeDecisions[node] = new Decision(
						Domain.IntegerNonnegative,
						"Node_" + node.Node.ID + "_expansion"
					);

					model.AddDecision(expandNodeDecisions[node]);

					if(node.Expansion == null)
						node.Expansion = new ExpansionParameters();

					var perUnitCost = node.Expansion.CapacityExpansionCostPerUnit ??
						(int)net.OptimizationResult.DefaultNodeExpansion.CapacityExpansionCostPerUnit;
					int maxPossible = node.Expansion.CapacityExpansionMaxPossible ??
						(int)net.OptimizationResult.DefaultNodeExpansion.CapacityExpansionMaxPossible;

					totalExpandCosts += expandNodeDecisions[node] * perUnitCost;

					model.AddConstraint("Node_exp_cap_" + node.Node.ID,
						expandNodeDecisions[node] <= maxPossible);
				}
			}
			if(options.AllowLocomotiveCapacityExpansion)
			{
				foreach(var link in net.OptimizationResult.Links)
				{
					expandLinkDecisions[link] = new Decision(
						Domain.IntegerNonnegative,
						"Link_" + link.Link.ID + "_expansion"
					);

					model.AddDecision(expandLinkDecisions[link]);

					if(link.Expansion == null)
						link.Expansion = new ExpansionParameters();

					var perUnitCost = link.Expansion.CapacityExpansionCostPerUnit ??
						(int)net.OptimizationResult.DefaultLinkExpansion.CapacityExpansionCostPerUnit;
					int maxPossible = link.Expansion.CapacityExpansionMaxPossible ??
						(int)net.OptimizationResult.DefaultLinkExpansion.CapacityExpansionMaxPossible;

					totalExpandCosts += expandLinkDecisions[link] * perUnitCost;

					model.AddConstraint("Link_exp_cap_" + link.Link.ID,
						expandLinkDecisions[link] <= maxPossible);
				}
			}
			#endregion

			#region Locomotive decision variables, locomotive costs
			Term totalLocomotiveCosts = 0;
			var locomotiveDecisions = new Dictionary<Link, Decision>();
			foreach(var link in links)
			{
				locomotiveDecisions[link] = new Decision(Domain.IntegerNonnegative, 
					"Locomotives_" + link.From.ID + "_to_" + link.To.ID);
				model.AddDecision(locomotiveDecisions[link]);

				// Constraint on max locomotives.
				// If optimized, number of locomotives is allowed to be below the capacity.
				Term locos = locomotiveDecisions[link];
				if(options.AllowLocomotiveCapacityExpansion)
				{
					Decision expand = expandLinkDecisions.FirstOrDefault(
						l => l.Key.Link == link
					).Value;

					model.AddConstraint("Maxloco_" + link.From.ID + "_to_" + link.To.ID,
						locos <= link.MaxTrains + expand);
				}
				else
				{
					model.AddConstraint("Maxloco_" + link.From.ID + "_to_" + link.To.ID,
						locos <= link.MaxTrains);
				}

				Term locoCost = link.Distance * locomotiveDecisions[link]
					* (net.NonFuelCostPerMile + net.FuelCostPerMile * link.FuelAdjustment);
				totalLocomotiveCosts += locoCost;
			}
			#endregion

			#region Constraint 1: Order Flow
			// For loop so that indices are kept.
			foreach(var fulfiller in nodes)
			{
				foreach(var fulfilled in nodes)
				{
					var fulfilledOrders = orders.Where(o => o.Origin == fulfiller);
					if(fulfilled != fulfiller)
						fulfilledOrders = fulfilledOrders.Where(o => o.Destination == fulfilled);
					
					// Required flow on nodeFulfilled to satisfy the order.
					int requiredFlow = 0;
					foreach(Order order in fulfilledOrders)
					{
						if(fulfilled == fulfiller)
							requiredFlow += order.Cars;
						else
							requiredFlow -= order.Cars;
					}

					// Sum of decision variables that should match the required flow.
					Term actualFlow = 0;
					// Get links that go to and from fulfilled.
					var fromLinks = links.Where(l => l.From == fulfilled);
					var toLinks = links.Where(l => l.To == fulfilled);
					// Go through related decision variables.
					foreach(var link in fromLinks)
					{
						// Decision variable index.
						actualFlow += flowDecisions[fulfiller][link];
					}
					foreach(var link in toLinks)
					{
						// Decision variable index.
						actualFlow -= flowDecisions[fulfiller][link];
					}

					model.AddConstraint("_" + fulfiller.ID + "_fulfilling_" + fulfilled.ID,
						actualFlow == requiredFlow);
				}
			}
			#endregion

			#region Constraint 2: Link Capacity
			foreach(var link in links)
			{
				// Total amount of flow over this link, summed for all car sources.
				Term totalFlow = 0;

				foreach(var node in nodes)
				{
					totalFlow += flowDecisions[node][link];
				}

				Term linkCapacity = locomotiveDecisions[link] * net.MaxCarsPerTrain;
				if(options.AllowLocomotiveCapacityExpansion)
				{
					LinkOptimized optNode = net.OptimizationResult.Links.Where(n => n.Link == link).First();
					// If the user has set max possible expansions for flow in or out, use them.

					model.AddConstraint("Link_" + link.ID + "_cap",
						totalFlow <= linkCapacity + expandLinkDecisions[optNode]);
				}
				else
				{
					model.AddConstraint("Link_" + link.ID + "_cap",
						totalFlow <= linkCapacity);
				}
			}
			#endregion

			#region Constraint 3/4: Node Capacity
			foreach(var node in nodes)
			{
				// Get all in and out links.
				var outLinks = links.Where(l => l.From == node);
				var inLinks = links.Where(l => l.To == node);

				Term flowOut = 0;
				Term flowIn = 0;

				foreach(var nodeo in nodes)
				{
					foreach(var link in outLinks)
						flowOut += flowDecisions[nodeo][link];
					foreach(var link in inLinks)
						flowIn += flowDecisions[nodeo][link];
				}

				if(options.AllowNodeCapacityExpansion)
				{
					NodeOptimized optNode = net.OptimizationResult.Nodes.Where(n => n.Node == node).First();
					// If the user has set max possible expansions for flow in or out, use them.

					model.AddConstraint("Node_" + node.ID + "_capacity_out",
						flowOut <= node.CarCapacity + expandNodeDecisions[optNode]);
					model.AddConstraint("Node_" + node.ID + "_capacity_in",
						flowIn <= node.CarCapacity + expandNodeDecisions[optNode]);
					// If there is no max, make no constraint.
				}
				else
				{
					model.AddConstraint("Node_" + node.ID + "_capacity_out",
						flowOut <= node.CarCapacity);
					model.AddConstraint("Node_" + node.ID + "_capacity_in",
						flowIn <= node.CarCapacity);
				}
			}
			#endregion

			#region Objective Function
			// Objective function. (Minimize)
			Term totalCost = totalCarCosts + totalLocomotiveCosts + totalExpandCosts;
			model.AddGoal("TotalCost", GoalKind.Minimize, totalCost);
			#endregion

			//Directive directive = new LpSolveDirective();
			//Directive directive = new SimplexDirective();
			Directive directive = new GurobiDirective();
			//Directive directive = new MixedIntegerProgrammingDirective();

			directive.TimeLimit = TIMEOUT_MAX_SECONDS * 1000;

			var solution = context.Solve(directive);

			var extractedFlowDec = new Dictionary<Node, Dictionary<Link, int>>();
			var extractedLocoDec = new Dictionary<Link, int>();

			foreach(var nvp in flowDecisions)
			{
				extractedFlowDec[nvp.Key] = new Dictionary<Link,int>();
				foreach(var lvp in nvp.Value)
				{
					extractedFlowDec[nvp.Key][lvp.Key] = (int)lvp.Value.ToDouble();
				}
			}
			foreach(var lvp in locomotiveDecisions)
			{
				extractedLocoDec[lvp.Key] = (int)lvp.Value.ToDouble();
			}

			foreach(var nvp in expandNodeDecisions)
			{
				nvp.Key.ExpansionSuggested = (int)nvp.Value.ToDouble();
			}
			foreach(var lvp in expandLinkDecisions)
			{
				lvp.Key.ExpansionSuggested = (int)lvp.Value.ToDouble();
			}

			// Extract the suggestion cost. (Don't know a better way to do this.)
			double suggestionCapitalCost = 0.0;

			if(options.AllowNodeCapacityExpansion)
			{
				foreach(var node in net.OptimizationResult.Nodes)
				{
					var perUnitCost = node.Expansion.CapacityExpansionCostPerUnit ??
						(int)net.OptimizationResult.DefaultNodeExpansion.CapacityExpansionCostPerUnit;

					suggestionCapitalCost += expandNodeDecisions[node].ToDouble() * perUnitCost;
				}
			}
			if(options.AllowLocomotiveCapacityExpansion)
			{
				foreach(var link in net.OptimizationResult.Links)
				{
					var perUnitCost = link.Expansion.CapacityExpansionCostPerUnit ??
						(int)net.OptimizationResult.DefaultLinkExpansion.CapacityExpansionCostPerUnit;

					suggestionCapitalCost += expandLinkDecisions[link].ToDouble() * perUnitCost;
				}
			}

			RawOptimizedState.RawQuality q;
			switch(solution.Quality)
			{
				case SolverQuality.Optimal:
					q = RawOptimizedState.RawQuality.Solved;
					break;

				case SolverQuality.Infeasible:
					q = RawOptimizedState.RawQuality.Infeasible;
					break;

				case SolverQuality.InfeasibleOrUnbounded:
				case SolverQuality.Unbounded:
					q = RawOptimizedState.RawQuality.Unbounded;
					break;

				case SolverQuality.Unknown:
				case SolverQuality.Feasible:
				default:
					q = RawOptimizedState.RawQuality.TimedOut;
					break;
			}
			var rawState = new RawOptimizedState()
			{
				solvedNetwork = net,
				flowDecisions = extractedFlowDec,
				locomotiveDecisions = extractedLocoDec,
				totalCost = (int)(model.Goals.First().ToDouble() - suggestionCapitalCost),
				suggestionCapitalCost = (int)suggestionCapitalCost,
				Quality = q
			};

			return rawState;
		}

	}

}
