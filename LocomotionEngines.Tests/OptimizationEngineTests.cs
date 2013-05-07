using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using DataModel.Class;

namespace LocomotionEngines.Tests
{
	[TestClass]
	public class OptimizationEngineTests
	{
		[TestMethod]
		public void TestNewNetwork()
		{
			XmlEngine e = new XmlEngine();
			var network = e.XmlFileToNetwork(
				XDocument.Load("../../TestNetworks/NewSampleInputs.xml")
			);

			// Assign 1-based IDs to maintain validity.
			for(int i = 0; i < network.Nodes.Count; i++)
				network.Nodes.ElementAt(i).ID = i + 1;
			for(int i = 0; i < network.Links.Count; i++)
				network.Links.ElementAt(i).ID = i + 1;
			for(int i = 0; i < network.Orders.Count; i++)
				network.Orders.ElementAt(i).ID = i + 1;

			OptimizationEngine oe = OptimizationEngine.getInstance();
			var optimization = oe.Optimize(network,
				new OptimizationEngine.OptimizationOptions());

			var op = optimization.ToOptimization();

			// Can't get the excel spreadsheet to agree even if I give it the optimizer's shipping
			// decisions and train numbers.
			Assert.AreEqual(3343246, optimization.totalCost);

			Assert.Inconclusive();
		}

		[TestMethod]
		public void TestNewExample4()
		{
			XmlEngine e = new XmlEngine();
			var network = e.XmlFileToNetwork(
				XDocument.Load("../../TestNetworks/new_example4.xml")
			);

			// Assign 1-based IDs to check against the excel documents.
			for(int i = 0; i < network.Nodes.Count; i++)
				network.Nodes.ElementAt(i).ID = i + 1;
			for(int i = 0; i < network.Links.Count; i++)
				network.Links.ElementAt(i).ID = i + 1;
			for(int i = 0; i < network.Orders.Count; i++)
				network.Orders.ElementAt(i).ID = i + 1;

			network.Optimize();

			#region Expected values
			var expectedNodeDec = TranslateToNodes(new Dictionary<string, Dictionary<string, int>>()
				{
					{"LAX",
						new Dictionary<string, int>(){
							{"LAX>DEN", 560 },
							{"LAX>PHX", 440 },
							{"DEN>LAX", 0 },
							{"DEN>PHX", 0 },
							{"PHX>LAX", 0 },
							{"PHX>DEN", 35 },
						}
					},
					{"DEN",
						new Dictionary<string, int>(){
							{"LAX>DEN", 0 },
							{"LAX>PHX", 0 },
							{"DEN>LAX", 240 },
							{"DEN>PHX", 560 },
							{"PHX>LAX", 60 },
							{"PHX>DEN", 0 },
						}
					},
					{"PHX",
						new Dictionary<string, int>(){
							{"LAX>DEN", 0 },
							{"LAX>PHX", 0 },
							{"DEN>LAX", 0 },
							{"DEN>PHX", 0 },
							{"PHX>LAX", 100 },
							{"PHX>DEN", 100 },
						}
					}
				}, network);
			var expectedLinkDec = TranslateToLinks(new Dictionary<string, int>()
				{
					{"LAX>DEN", 7 },
					{"LAX>PHX", 6 },
					{"DEN>LAX", 3 },
					{"DEN>PHX", 7 },
					{"PHX>LAX", 2 },
					{"PHX>DEN", 2 },
				}, network);
			#endregion

			OptimizationEngine oe = OptimizationEngine.getInstance();
			var optimization = oe.Optimize(network,
				new OptimizationEngine.OptimizationOptions());

			var op = optimization.ToOptimization();

			Assert.IsTrue(NodeDictionariesEqual(expectedNodeDec, optimization.flowDecisions));

			Assert.IsTrue(
				LinkDictionariesEqual(expectedLinkDec, optimization.locomotiveDecisions));

			Assert.AreEqual(9917500, optimization.totalCost);
		}

		public Dictionary<Node, Dictionary<Link, int>> TranslateToNodes(
			Dictionary<string, Dictionary<string, int>> sol, Network net)
		{
			var r = new Dictionary<Node, Dictionary<Link, int>>();
			foreach(var node in sol)
			{
				var nodeActual = net.Nodes.Where(n => n.StationCode == node.Key).First();
				r[nodeActual] = new Dictionary<Link,int>();
				foreach(var link in node.Value)
				{
					string[] nodeNames = link.Key.Split('>');
					var linkActual = net.Links.Where(
						n => n.From.StationCode == nodeNames[0]
						&& n.To.StationCode == nodeNames[1]).First();

					r[nodeActual][linkActual] = link.Value;
				}
			}
			return r;
		}

		public Dictionary<Link, int> TranslateToLinks(
			Dictionary<string, int> sol, Network net)
		{
			var r = new Dictionary<Link, int>();
			foreach(var link in sol)
			{
				string[] nodeNames = link.Key.Split('>');
				var linkActual = net.Links.Where(
					n => n.From.StationCode == nodeNames[0]
					&& n.To.StationCode == nodeNames[1]).First();

				r[linkActual] = link.Value;
			}
			return r;
		}

		public bool NodeDictionariesEqual(
			Dictionary<Node, Dictionary<Link, int>> expected,
			Dictionary<Node, Dictionary<Link, int>> actual)
		{
			foreach(var n in expected)
			{
				foreach(var l in n.Value)
				{
					if(l.Value != actual[n.Key][l.Key])
						return false;
				}
			}
			return true;
		}

		public bool LinkDictionariesEqual(
			Dictionary<Link, int> expected,
			Dictionary<Link, int> actual)
		{
			foreach(var l in expected)
			{
				if(l.Value != actual[l.Key])
					return false;
			}
			return true;
		}

	}
}
