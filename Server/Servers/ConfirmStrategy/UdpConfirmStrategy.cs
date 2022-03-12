using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;

namespace Server.Servers.ConfirmStrategy
{
	internal class UdpConfirmStrategy : IConfirmStrategy
	{
		private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

		private readonly UdpClient _confirmUdpClient;
		private readonly IPEndPoint _clientIpEndPoint;

		public UdpConfirmStrategy(IPEndPoint clientIpEndPoint)
		{
			_confirmUdpClient = new UdpClient();
			_clientIpEndPoint = clientIpEndPoint;
		}

		public void Confirm(FileBatch fileBatch)
		{
			var stream = new MemoryStream();
			BinaryFormatter.Serialize(stream, new ConfirmMessage { BatchOrder = fileBatch.Order });
			stream.Position = 0;

			var bytes = new byte[stream.Length];
			_ = stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

			_confirmUdpClient.Send(bytes, bytes.Length, _clientIpEndPoint);
		}

		public void StopConfirming()
		{
			_confirmUdpClient.Close();
		}

		public void Dispose()
		{
			_confirmUdpClient?.Close();
		}
	}
}
