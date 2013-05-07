using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Enum
{
	public enum UserAccessGroup
	{
		Administrator, // All rights
		Operations, // Editing/simulation rights
		Engineering, // View rights
		Financials,
		Other,

	}

	public enum StochasticType
	{
		NonStochastic = 0,
		FivePercentUniform1 = 1,
		FivePercentUniform2 = 2
	}
}
