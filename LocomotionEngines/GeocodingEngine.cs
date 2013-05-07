using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DataModel.Class;

namespace LocomotionEngines
{
	public class GeocodingEngine
	{
		private static GeocodingEngine instance;
		public static GeocodingEngine getInstance()
		{
			if(instance == null)
				instance = new GeocodingEngine();
			return instance;
		}
		private GeocodingEngine() {}

		public string LocationCountryCode(Point location)
		{
			string url = "http://maps.googleapis.com/maps/api/geocode/xml?" +
				string.Format("latlng={0},{1}&sensor=false", location.Latitude, location.Longitude);

			using(WebClient client = new WebClient())
			{
				XDocument result = XDocument.Parse(client.DownloadString(url));

				try
				{
					return result.Descendants("result").First()
						.Descendants("address_component").Where(
							c => c.Descendants("type").Count(d => d.Value == "country") > 0
						)
						.Descendants("short_name").First().Value;
				}
				catch(Exception e)
				{
					// Use USA as fallback.
					return "USA";
				}
			}
		}

		public string CountryCodeCurrency(string country)
		{
			string url = "http://www.oorsprong.org/websamples.countryinfo/CountryInfoService.wso/" + 
				string.Format("CountryCurrency?sCountryISOCode={0}", country);

			using(WebClient client = new WebClient())
			{
				XDocument result = XDocument.Parse(client.DownloadString(url));

				return result.Descendants("sISOCode").First().Value;
			}
		}

		public double LocationConvertToUSD(Point location, double moneyAmount)
		{
			string locCurrency = CountryCodeCurrency(LocationCountryCode(location));

			string url = string.Format("http://www.geoplugin.net/xml.gp?base_currency={0}", locCurrency);

			using(WebClient client = new WebClient())
			{
				try
				{
					XDocument result = XDocument.Parse(client.DownloadString(url));

					double conversion = double.Parse(
						result.Descendants("geoplugin_currencyConverter").First().Value);

					return moneyAmount * conversion;
				}
				catch(Exception)
				{
					// As a fallback, don't convert at all.
					return moneyAmount;
				}
			}
		}

		// Adapted from http://www.codecodex.com/wiki/Calculate_Distance_Between_Two_Points_on_a_Globe
		public double Distance(Point pos1, Point pos2)
		{
			double R = 3960;

			double dLat = ToRadian(pos2.Latitude - pos1.Latitude);
			double dLon = ToRadian(pos2.Longitude - pos1.Longitude);

			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(ToRadian(pos1.Latitude)) *
				Math.Cos(ToRadian(pos2.Latitude)) *
				Math.Sin(dLon / 2) *
				Math.Sin(dLon / 2);

			double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
			double d = R * c;
			return d;
		}

		private double ToRadian(double val)
		{
			return (Math.PI / 180) * val;
		}
	}
}
