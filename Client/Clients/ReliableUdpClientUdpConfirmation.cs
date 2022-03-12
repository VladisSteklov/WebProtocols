using System.Net;

using Client.Clients.DeliveryConfirmationManager;

using WebProtocolsModel;

namespace Client.Clients
{
	internal class ReliableUdpClientUdpConfirmation : ReliableUdpClient
	{
		public ReliableUdpClientUdpConfirmation(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
		{
		}

		
		protected override IReadConfirmationsStrategyFactory CreateConfirmationsStrategyFactory()
		{
			var udpConfirmationServer = new System.Net.Sockets.UdpClient(
				new IPEndPoint(ServerIpEndPoint.Address, ServerContext.ConfirmationPort));

			return new UdpReadConfirmationsStrategyFactory(udpConfirmationServer);
		}
	}
}
