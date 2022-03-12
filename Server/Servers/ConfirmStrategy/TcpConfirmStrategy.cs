using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;

namespace Server.Servers.ConfirmStrategy
{
	internal class TcpConfirmStrategy : IConfirmStrategy
	{
		private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

		private readonly TcpClient _confirmingClient;

		private readonly Stream _confirmingStream;

		internal TcpConfirmStrategy(TcpClient confirmingClient, NetworkStream networkStream)
		{
			_confirmingClient = confirmingClient;
			_confirmingStream = networkStream;
		}

		public void Confirm(FileBatch fileBatch)
		{
			var stream = new MemoryStream();
			BinaryFormatter.Serialize(stream, new ConfirmMessage { BatchOrder = fileBatch.Order });
			stream.Position = 0;

			var bytes = new byte[stream.Length];
			_ = stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

			_confirmingStream.Write(bytes, 0, bytes.Length);
		}

		public void StopConfirming()
		{
			_confirmingStream.Close();
			_confirmingClient.Close();
		}

		public void Dispose()
		{
			_confirmingStream?.Dispose();
			_confirmingClient?.Dispose();
		}
	}
}
