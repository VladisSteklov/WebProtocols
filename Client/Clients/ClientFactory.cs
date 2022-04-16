using WebProtocolsModel;

namespace Client.Clients
{
	internal static class ClientFactory
	{
		internal const char TcpClientKey = 't';
		internal const char UdpClientKey = 'u';
		internal const char ReliableUdpClientTcpConfirmationKey = 'r';
		internal const char ReliableUdpClientUdpConfirmationKey = 'q';
		internal const char StreamSocketClientKey = 's';
		internal const char DgramSocketClientKey = 'd';
		internal const char ProtocolVolatileWebClient = 'v';

		internal static Client TryCreateClient(char key)
		{
			return key switch
			{
				TcpClientKey => new TcpClient(ServerContext.Address, ServerContext.Port),
				UdpClientKey => new UdpClient(ServerContext.Address, ServerContext.Port),
				ReliableUdpClientTcpConfirmationKey => new ReliableUdpClientTcpConfirmation(ServerContext.Address, ServerContext.Port),
				ReliableUdpClientUdpConfirmationKey => new ReliableUdpClientUdpConfirmation(ServerContext.Address, ServerContext.Port),
				StreamSocketClientKey => new StreamSocketClient(ServerContext.Address, ServerContext.Port),
				DgramSocketClientKey => new DgramSocketClient(ServerContext.Address, ServerContext.Port),
				ProtocolVolatileWebClient => new ProtocolVolatileWebClient.ProtocolVolatileWebClient(ServerContext.Address, ServerContext.Port),
				_ => null
			};
		}
	}
}
