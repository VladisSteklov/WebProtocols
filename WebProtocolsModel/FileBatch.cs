using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebProtocolsModel
{
	[Serializable]
	public class FileBatch
	{
		public byte[] Bytes { get; set; }

		public int Order { get; set; }
	}
}
