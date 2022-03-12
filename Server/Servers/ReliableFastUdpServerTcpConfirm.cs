using Server.Servers.ConfirmStrategy.ConfirmStrategyFactory;

namespace Server.Servers
{
	internal class ReliableFastUdpServerTcpConfirm : ReliableFastUdpServer
	{
		internal ReliableFastUdpServerTcpConfirm(string serverAddress, int serverPort)
			: base(serverAddress, serverPort, new TcpConfirmStrategyFactory())
		{
		}
	}
}
