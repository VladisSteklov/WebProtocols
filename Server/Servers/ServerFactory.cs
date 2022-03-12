using System;
using WebProtocolsModel;

namespace Server.Servers
{
	internal static class ServerFactory
	{
		internal const char TcpServerKey = 't';
		internal const char UdpServerKey = 'u';
		internal const char ReliableSlowUdpServerTcpConfirmServerKey = 'r';
		internal const char ReliableFastUdpServerTcpConfirmServerKey = 'f';
		internal const char StreamSocketServerKey = 's';
		internal const char DgramSocketServerKey = 'd';

		internal static Server TryCreateServer(char key)
		{
			switch (key)
			{
				case TcpServerKey:
					return new TcpServer(ServerContext.Address, ServerContext.Port);
				case UdpServerKey:
					return new UdpServer(ServerContext.Address, ServerContext.Port);
				case ReliableSlowUdpServerTcpConfirmServerKey:
					return new ReliableSlowUdpServerTcpConfirm(ServerContext.Address, ServerContext.Port);
				case ReliableFastUdpServerTcpConfirmServerKey:
					return new ReliableFastUdpServerTcpConfirm(ServerContext.Address ,ServerContext.Port);
				case StreamSocketServerKey:
					return new StreamSocketServer(ServerContext.Address, ServerContext.Port);
				case DgramSocketServerKey:
					return new DgramSocketServer(ServerContext.Address, ServerContext.Port);
				default:
					return null;
			}
		}
	}
}
