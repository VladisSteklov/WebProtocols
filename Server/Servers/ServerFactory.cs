using System;
using WebProtocolsModel;

namespace Server.Servers
{
	internal static class ServerFactory
	{
		internal const char TcpServerKey = 't';
		internal const char UdpServer = 'u';
		internal const char ReliableUdpServer = 'r';
		internal const char StreamSocketServer = 's';
		internal const char DgramSocketServer = 'd';

		internal static Server TryCreateServer(char key)
		{
			switch (key)
			{
				case TcpServerKey:
					return new TcpServer(ServerContext.Address, ServerContext.Port);
				case UdpServer:
					return new UdpServer(ServerContext.Address, ServerContext.Port);
				case ReliableUdpServer:
					return new ReliableUdpServer(ServerContext.Address, ServerContext.Port);
				case StreamSocketServer:
					return new StreamSocketServer(ServerContext.Address, ServerContext.Port);
				case DgramSocketServer:
					return new DgramSocketServer(ServerContext.Address, ServerContext.Port);
				default:
					return null;
			}
		}
	}
}
