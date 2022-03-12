using Server.Servers.ConfirmStrategy.ConfirmStrategyFactory;


namespace Server.Servers
{
	internal class ReliableFastUdpServerUdpConfirm : ReliableFastUdpServer
	{
		public ReliableFastUdpServerUdpConfirm(string serverAddress, int serverPort)
			: base(serverAddress, serverPort, new UdpConfirmStrategyFactory())
		{
		}
	}
}
