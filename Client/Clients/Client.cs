using System;
using System.Net;
using WebProtocolsModel;

namespace Client.Clients
{
	public abstract class Client
	{
		protected readonly IPAddress serverAddress;
		protected readonly int serverPort;

		protected int bufferSize = ServerContext.BufferSize;

		public Client(string serverAddress, int serverPort)
		{
			if (!IPAddress.TryParse(serverAddress, out var iPAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}

			this.serverAddress = iPAddress;
			this.serverPort = serverPort;
		}

		public abstract void SendFile(string fileName);
	}
}
