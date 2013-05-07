using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.Class;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Net;

namespace LocomotionEngines
{
	public class XmlEngine
	{
		public Network UrlDownloadToNetwork(Uri nUri)
		{
			using(WebClient client = new WebClient())
			{
				XDocument inFile = XDocument.Parse(client.DownloadString(nUri));				
				return XmlFileToNetwork(inFile);
			}
		}

		public Network XmlFileToNetwork(XDocument inFile)
		{
			var nodes = new List<Node>();
			var links = new List<Link>();
			var orders = new List<Order>();
			int maxCars = 0;
			double nonFuelCost = 0, fuelCost = 0, carCost = 0;	
			var nodeCurAdjMap = new Dictionary<Node, double>();
			int i =0;
			foreach (XElement element in inFile.Root.Elements())
			{
				i++;				
				switch (element.Name.ToString())
				{
					case "Network":									
					foreach (XAttribute a in element.Attributes())
					{
						switch (a.Name.ToString())
						{
							case "maxCars":
								maxCars = int.Parse(a.Value);
								break;
							case "nonFuelCost":
								nonFuelCost = Double.Parse(a.Value);
								break;
							case "fuelCost":
								fuelCost = Double.Parse(a.Value);
								break;
							case "carCost":
								carCost = Double.Parse(a.Value);
								break;
						}
					}
					foreach (XElement el in element.Elements())
					{
						switch(el.Name.ToString())
						{
							case "Nodes":
							foreach (XElement e in el.Elements())
							{
								string stationId = "", name = "";
								int carCap = 0; //outCapacity = 0;
								double latitude = 0, longitude = 0;
								foreach (XAttribute att in e.Attributes())
								{
									switch (att.Name.ToString())
									{
										case "id":
											stationId = att.Value;
											break;
										case "cars":
											carCap = int.Parse(att.Value);
											break;
										case "latitude":
											latitude = Double.Parse(att.Value);
											break;
										case "longitude":
											longitude = Double.Parse(att.Value);
											break;
									}
								}
								Node temp = new Node()
								{
									Name = name,
									StationCode = stationId,
									Location = new Point() { Latitude = latitude, Longitude = longitude },
									CarCapacity = carCap,
									InLinks = new List<Link>(),
									OutLinks = new List<Link>(),
									InOrders = new List<Order>(),
									OutOrders = new List<Order>(),
								};
								nodeCurAdjMap[temp] = GeocodingEngine.getInstance()
									.LocationConvertToUSD(temp.Location, 1.0);
								nodes.Add(temp);
							}
							break; //end Nodes
							case "Arcs":							
							foreach (XElement e in el.Elements())
							{
								string to ="", from ="";
								double track_mult = 0, fuel_adj =0;
								int max_trains = 0;
								foreach (XAttribute att in e.Attributes())
								{
									switch (att.Name.ToString())
									{
										case "end":
											to = att.Value;
											break;

										case "start":
											from = att.Value;
											break;
										case "trackMultiplier":
											track_mult = Double.Parse(att.Value);
											break;
										case "maxTrains":
											max_trains = Int32.Parse(att.Value);
											break;
										case "fuelAdj":
											fuel_adj = Double.Parse(att.Value);
											break;
									}
								}
								int toIndex = -1;
								int fromIndex = -1;
								int k = 0;
								foreach (Node n in nodes)
								{
									if (toIndex >= 0 && fromIndex >= 0)
										break;
									if (n.StationCode.Equals(to))
										toIndex = k;
									if (n.StationCode.Equals(from))
										fromIndex = k;
									k++;
								}
								Link tempLink = new Link
								{                                
									From = nodes[fromIndex],
									To = nodes[toIndex],
									MaxTrains = max_trains,
									FuelAdjustment = fuel_adj,
								};
								tempLink.Distance = GeocodingEngine.getInstance().Distance(
									tempLink.From.Location,
									tempLink.To.Location
								) * track_mult;
								nodes[fromIndex].OutLinks.Add(tempLink);
								nodes[toIndex].InLinks.Add(tempLink);
								links.Add(tempLink);
							}
						break; //end arcs
						} 
					}
					break; //end network
					case "Orders":
					foreach (XElement el in element.Elements())
					{
						string id ="", origin ="", dest ="";
						int cars = 0;
						double revenue = 0;
						foreach (XAttribute att in el.Attributes())
						{							
							switch(att.Name.ToString())
							{
								case "cars":
									cars = int.Parse(att.Value);
									break;
								case "revenue":
									revenue = Double.Parse(att.Value);
									break;
								case "to":
									dest = att.Value;
									break;
								case "from":
									origin = att.Value;
									break;
							}
						}
						int destIndex = -1;
						int origIndex = -1;
						int k = 0;
						foreach (Node n in nodes)
						{
							if (destIndex >= 0 && origIndex >= 0)
								break;
							if(n.StationCode.Equals(dest))
								destIndex = k;
							if(n.StationCode.Equals(origin))
								origIndex = k;
							k++;
						}
						Order tempOrder = new Order { Cars = cars,
							Destination = nodes[destIndex], Origin = nodes[origIndex],
							//XMLOrderID = 
						};
						tempOrder.Revenue = nodeCurAdjMap[tempOrder.Origin] * revenue;
						nodes[origIndex].OutOrders.Add(tempOrder);
						nodes[destIndex].InOrders.Add(tempOrder);
						orders.Add(tempOrder);

					}
					break; //end orders
				}
			}	

			var network = new Network{ CarCostPerMile = carCost,
				FuelCostPerMile = fuelCost, NonFuelCostPerMile = nonFuelCost,
				MaxCarsPerTrain = maxCars, Links = links,
				Nodes = nodes, Orders = orders,
				OptimizationResult = null};

			return network;
		}
	}
}
