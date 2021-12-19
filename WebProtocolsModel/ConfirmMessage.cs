using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebProtocolsModel
{
	[Serializable]
	public class ConfirmMessage
	{
		public int BatchOrder { get; set; }
	}
}
