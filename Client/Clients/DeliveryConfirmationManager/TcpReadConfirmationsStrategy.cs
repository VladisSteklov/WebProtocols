using System;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using WebProtocolsModel;

namespace Client.Clients.DeliveryConfirmationManager
{
	internal sealed class TcpReadConfirmationsStrategy : IReadConfirmationsStrategy
	{
		private readonly System.Net.Sockets.TcpClient _acceptClient;

		private readonly NetworkStream _networkStream;

		private static readonly BinaryFormatter BinaryFormatter = new();

		internal TcpReadConfirmationsStrategy(System.Net.Sockets.TcpClient acceptClient, NetworkStream networkStream)
		{
			_acceptClient = acceptClient;
			_networkStream = networkStream;
		}

		public ConfirmMessage TryReadConfirmation()
		{
			if (!_networkStream.DataAvailable)
				return null;

			if (!(BinaryFormatter.Deserialize(_networkStream) is ConfirmMessage confirmMessage))
				throw new InvalidCastException($"Message from network stream is not {nameof(ConfirmMessage)}");

			return confirmMessage;
		}

		public void Dispose()
		{
			_acceptClient?.Close();
			_networkStream?.Close();
		}
	}
}
