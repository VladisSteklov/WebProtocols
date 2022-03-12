using System;
using System.Net;

using WebProtocolsModel;

namespace Server.Servers
{
	internal abstract class Server
	{
		protected int BufferSize => ServerContext.BufferSize;

		protected IPAddress ServerIpAddress { get; }

		protected Server(string serverAddress)
		{
			if (!IPAddress.TryParse(serverAddress, out var serverIpAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}
			ServerIpAddress = serverIpAddress;
		}

		public abstract void Process();
	}
}
