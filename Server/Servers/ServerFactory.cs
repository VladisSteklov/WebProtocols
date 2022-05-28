using System;
using Server.Servers.MultipleTransmissions;
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
		internal const char ReliableFastUdpServerUdpConfirmServerKey = 'q';
		internal const char ProtocolVolatileWebSever = 'v';
		internal const char UseOneConnectionTcpServerKey = 'o';
		internal const char UseConnectionByFileTcpServerKey = 'm';

		internal static Server TryCreateServer(char key)
		{
			return key switch
			{
				TcpServerKey => new TcpServer(ServerContext.Address, ServerContext.Port),
				UdpServerKey => new UdpServer(ServerContext.Address, ServerContext.Port),
				ReliableSlowUdpServerTcpConfirmServerKey => new ReliableSlowUdpServerTcpConfirm(ServerContext.Address,
					ServerContext.Port),
				ReliableFastUdpServerTcpConfirmServerKey => new ReliableFastUdpServerTcpConfirm(ServerContext.Address,
					ServerContext.Port),
				StreamSocketServerKey => new StreamSocketServer(ServerContext.Address, ServerContext.Port),
				DgramSocketServerKey => new DgramSocketServer(ServerContext.Address, ServerContext.Port),
				ReliableFastUdpServerUdpConfirmServerKey => new ReliableFastUdpServerUdpConfirm(ServerContext.Address,
					ServerContext.Port),
				ProtocolVolatileWebSever => new ProtocolVolatileWebServer.ProtocolVolatileWebServer(ServerContext.Address, ServerContext.Port),
				UseOneConnectionTcpServerKey => new UseOneConnectionTcpServer(ServerContext.Address, ServerContext.Port),
				UseConnectionByFileTcpServerKey => new UseConnectionByFileTcpServer(ServerContext.Address, ServerContext.Port),
				_ => null
			};
		}
	}
}
