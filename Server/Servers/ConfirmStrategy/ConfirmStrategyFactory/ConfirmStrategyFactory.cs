using System.Net;
using System.Net.Sockets;

using WebProtocolsModel;

namespace Server.Servers.ConfirmStrategy.ConfirmStrategyFactory
{
	internal interface IConfirmStrategyFactory
	{ 
		IConfirmStrategy CreateStrategy(IPAddress ipServerAddress);
	}

	internal class TcpConfirmStrategyFactory : IConfirmStrategyFactory
	{
		public IConfirmStrategy CreateStrategy(IPAddress ipServerAddress)
		{
			var confirmingClient = new TcpClient();

			confirmingClient.Connect(ipServerAddress, ServerContext.ConfirmationPort);

			return new TcpConfirmStrategy(confirmingClient, confirmingClient.GetStream());
		}
	}

	internal class UdpConfirmStrategyFactory : IConfirmStrategyFactory
	{
		public IConfirmStrategy CreateStrategy(IPAddress ipServerAddress)
		{
			return new UdpConfirmStrategy(new IPEndPoint(ipServerAddress, ServerContext.ConfirmationPort));
		}
	}
}
