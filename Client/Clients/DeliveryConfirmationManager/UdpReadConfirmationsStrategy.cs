using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using WebProtocolsModel;

namespace Client.Clients.DeliveryConfirmationManager
{
	internal class UdpReadConfirmationsStrategy : IReadConfirmationsStrategy
	{
		private readonly System.Net.Sockets.UdpClient _udpConfirmationServer;

		public UdpReadConfirmationsStrategy(System.Net.Sockets.UdpClient udpConfirmationServer)
		{
			_udpConfirmationServer = udpConfirmationServer;
		}

		public ConfirmMessage TryReadConfirmation()
		{
			if (_udpConfirmationServer.Available == 0)
				return null;

			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

			var data = _udpConfirmationServer.Receive(ref remoteEndPoint);

			var memory = new MemoryStream();
			memory.Write(data, 0, data.Length);
			memory.Position = 0;

			var binaryFormatter = new BinaryFormatter();

			var confirmMessage = (ConfirmMessage)binaryFormatter.Deserialize(memory);
			return confirmMessage;
		}

		public void Dispose()
		{
			_udpConfirmationServer?.Dispose();
		}
	}
}
