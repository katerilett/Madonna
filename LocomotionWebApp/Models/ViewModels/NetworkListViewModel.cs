using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataModel.Class;

namespace LocomotionWebApp.Models.ViewModels
{
	public class NetworkListViewModel
	{
		public ICollection<Network> Networks { get; set; }
	}
}