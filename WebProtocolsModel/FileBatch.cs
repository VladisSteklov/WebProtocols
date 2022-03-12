using System;
using System.Collections.Generic;
using System.IO;
namespace WebProtocolsModel
{
	[Serializable]
	public class FileBatch
	{
		public byte[] Bytes { get; set; }

		public int Order { get; set; }
	}
}
