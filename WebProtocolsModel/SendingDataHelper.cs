using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace WebProtocolsModel
{
	public static class SendingDataHelper
	{
		public static byte[] PrepareData(object obj)
		{
			var binaryFormatter = new BinaryFormatter();

			var stream = new MemoryStream();
			binaryFormatter.Serialize(stream, obj);
			stream.Position = 0;

			var bytes = new byte[stream.Length];
			stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

			return bytes;
		}
	}
}
