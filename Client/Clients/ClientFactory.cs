using WebProtocolsModel;

namespace Client.Clients
{
	internal static class ClientFactory
	{
		internal const char TcpClientKey = 't';
		internal const char UdpClient = 'u';
		internal const char ReliableUdpClient = 'r';
		internal const char StreamSocketClient = 's';
		internal const char DgramSocketClient = 'd';

		internal static Client TryCreateClient(char key)
		{
			switch(key)
			{
				case TcpClientKey:
					return new TcpClient(ServerContext.Address, ServerContext.Port);
				case UdpClient:
					return new UdpClient(ServerContext.Address, ServerContext.Port);
				case ReliableUdpClient:
					return new ReliableUdpClient(ServerContext.Address, ServerContext.Port);
				case StreamSocketClient:
					return new StreamSocketClient(ServerContext.Address, ServerContext.Port);
				case DgramSocketClient:
					return new DgramSocketClient(ServerContext.Address, ServerContext.Port);
				default:
					return null;
			}
		}
	}
}
