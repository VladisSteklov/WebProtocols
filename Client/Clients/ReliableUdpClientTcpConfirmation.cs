using System.Net.Sockets;
using Client.Clients.DeliveryConfirmationManager;
using WebProtocolsModel;

namespace Client.Clients
{
	internal class ReliableUdpClientTcpConfirmation : ReliableUdpClient
	{
		public ReliableUdpClientTcpConfirmation(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
		{
		}

		protected override IReadConfirmationsStrategyFactory CreateConfirmationsStrategyFactory()
		{
			var deliveryConfirmationTcpListener = new TcpListener(ServerIpEndPoint.Address, ServerContext.ConfirmationPort);
			deliveryConfirmationTcpListener.Start();

			return new TcpReadConfirmationsStrategyFactory(deliveryConfirmationTcpListener);
		}
	}
}
