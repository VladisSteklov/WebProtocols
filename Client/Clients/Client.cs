using System;
using System.Net;

using WebProtocolsModel;

namespace Client.Clients
{
	public abstract class Client : IDisposable
	{
		protected int BufferSize => ServerContext.BufferSize;

		protected IPEndPoint ServerIpEndPoint { get; }

		internal Client(string serverIpAddress, int serverPort)
		{
			if (!IPAddress.TryParse(serverIpAddress, out var iPAddress))
			{
				throw new InvalidOperationException($"String {serverIpAddress} is not ip address");
			}

			ServerIpEndPoint = new IPEndPoint(iPAddress, serverPort);
		}

		public abstract void Dispose();
	}
}
