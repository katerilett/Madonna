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
	public class GeocodingEngineTests
	{
		public static double DistanceAllowableError = 1.0;

		[TestMethod]
		public void TestUSLoc()
		{
			// Convert 100 USD to Denver's currency (USD). Shouldn't change it.
			Assert.AreEqual(100.0,
				GeocodingEngine.getInstance().LocationConvertToUSD(
					new Point(){ Latitude = 39.7392, Longitude = -104.9842 }, 100.0
				)
			);
		}

		[TestMethod]
		public void TestInternational()
		{

			var peso100toUSD = GeocodingEngine.getInstance().LocationConvertToUSD(
				new Point(){ Latitude = 19.1300, Longitude = -99.4000 }, 100.0
			);

			var cad100toUSD = GeocodingEngine.getInstance().LocationConvertToUSD(
				new Point(){ Latitude = 49.2505, Longitude = -123.1119 }, 100.0
			);

			Assert.Inconclusive(string.Format("\n100 peso in USD: {0:C}\n100 CAD in USD: {1:C}",
				peso100toUSD, cad100toUSD));
		}
	}
}
