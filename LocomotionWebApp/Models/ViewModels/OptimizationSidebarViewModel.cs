using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataModel.Class;

namespace LocomotionWebApp.Models.ViewModels
{
	public class OptimizationSidebarViewModel
	{
		public Optimization Optimization { get; set; }
		public int StartTab { get; set; }
	}
}