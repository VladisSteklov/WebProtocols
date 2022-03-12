using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WebProtocolsModel
{
	public static class ObjectExtensions
	{
		public static byte[] ToByteArray(this object obj)
		{
			var stream = new MemoryStream();
			new BinaryFormatter().Serialize(stream, obj);
			stream.Position = 0;

			var bytes = new byte[stream.Length];
			_ = stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

			return bytes;
		}
	}
}
