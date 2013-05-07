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
	public class XMLEngineTests
	{
		public static double DistanceAllowableError = 1.0;

		[TestMethod]
		public void TestExcelSampleInput()
		{
			XmlEngine e = new XmlEngine();
			var network = e.XmlFileToNetwork(
				XDocument.Load("../../TestNetworks/NewSampleInputs_orig.xml")
			);

			#region Check link information
			// Rely on order to match links.
			Link[] expectedLinks = new Link[]
			{
				new Link() { Distance = 681, MaxTrains = 10, FuelAdjustment = 1 },
				new Link() { Distance = 842, MaxTrains = 2, FuelAdjustment = 1.4 },

				new Link() { Distance = 681, MaxTrains = 10, FuelAdjustment = 1 },
				new Link() { Distance = 735, MaxTrains = 8, FuelAdjustment = 1.4 },
				new Link() { Distance = 367, MaxTrains = 10, FuelAdjustment = 1 },

				new Link() { Distance = 367, MaxTrains = 10, FuelAdjustment = 1 },
				new Link() { Distance = 689, MaxTrains = 12, FuelAdjustment = 1.4 },
				new Link() { Distance = 500, MaxTrains = 16, FuelAdjustment = 1 },

				new Link() { Distance = 500, MaxTrains = 16, FuelAdjustment = 1 },
				new Link() { Distance = 773, MaxTrains = 10, FuelAdjustment = 1.2 },

				new Link() { Distance = 842, MaxTrains = 2, FuelAdjustment = 1.4 },
				new Link() { Distance = 735, MaxTrains = 8, FuelAdjustment = 1.4 },
				new Link() { Distance = 689, MaxTrains = 12, FuelAdjustment = 1.4 },
				new Link() { Distance = 773, MaxTrains = 10, FuelAdjustment = 1.2 },
			};
			int linkI = 0;
			foreach(var l in network.Links)
			{
				double actualDist = l.Distance;
				double expectedDist = expectedLinks[linkI].Distance;

				Assert.IsTrue(ApproxEqual(actualDist, expectedDist, DistanceAllowableError));

				Assert.AreEqual(l.MaxTrains, expectedLinks[linkI].MaxTrains);
				Assert.AreEqual(l.FuelAdjustment, expectedLinks[linkI].FuelAdjustment);

				linkI++;
			}
			#endregion
		}

		[TestMethod]
		public void TestOrigSampleInput()
		{
			XmlEngine e = new XmlEngine();
			var network = e.XmlFileToNetwork(XDocument.Load("../../TestNetworks/NewSampleInputs.xml"));

			#region Check link information
			// Rely on order to match links.
			Link[] expectedLinks = new Link[]
			{
				new Link() { Distance = 681, MaxTrains = 10, FuelAdjustment = 1 },
				new Link() { Distance = 842, MaxTrains = 2, FuelAdjustment = 1 },

				new Link() { Distance = 681, MaxTrains = 10, FuelAdjustment = 1 },
				new Link() { Distance = 735, MaxTrains = 8, FuelAdjustment = 1 },
				new Link() { Distance = 367, MaxTrains = 10, FuelAdjustment = 1 },

				new Link() { Distance = 367, MaxTrains = 10, FuelAdjustment = 1 },
				new Link() { Distance = 689, MaxTrains = 12, FuelAdjustment = 1 },
				new Link() { Distance = 500, MaxTrains = 16, FuelAdjustment = 1 },

				new Link() { Distance = 500, MaxTrains = 16, FuelAdjustment = 1 },
				new Link() { Distance = 773, MaxTrains = 10, FuelAdjustment = 1 },

				new Link() { Distance = 842, MaxTrains = 2, FuelAdjustment = 1 },
				new Link() { Distance = 735, MaxTrains = 8, FuelAdjustment = 1 },
				new Link() { Distance = 689, MaxTrains = 12, FuelAdjustment = 1 },
				new Link() { Distance = 773, MaxTrains = 10, FuelAdjustment = 1 },
			};
			int linkI = 0;
			foreach(var l in network.Links)
			{
				double actualDist = l.Distance;
				double expectedDist = expectedLinks[linkI].Distance;

				Assert.IsTrue(ApproxEqual(actualDist, expectedDist, DistanceAllowableError));

				Assert.AreEqual(l.MaxTrains, expectedLinks[linkI].MaxTrains);
				Assert.AreEqual(l.FuelAdjustment, expectedLinks[linkI].FuelAdjustment);

				linkI++;
			}
			#endregion
		}

		public bool ApproxEqual(double a, double b, double amt)
		{
			return a > b - amt && a < b + amt;
		}
	}
}
