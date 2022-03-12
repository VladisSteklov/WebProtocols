using System;
using System.Net.Sockets;

namespace Client.Clients.DeliveryConfirmationManager
{
	internal interface IReadConfirmationsStrategyFactory : IDisposable
	{
		IReadConfirmationsStrategy CreateStrategy();
	}

	internal class TcpReadConfirmationsStrategyFactory : IReadConfirmationsStrategyFactory
	{
		private readonly TcpListener _deliveryConfirmationTcpListener;

		public TcpReadConfirmationsStrategyFactory(TcpListener deliveryConfirmationTcpListener)
		{
			_deliveryConfirmationTcpListener = deliveryConfirmationTcpListener;
		}

		public IReadConfirmationsStrategy CreateStrategy()
		{
			var client = _deliveryConfirmationTcpListener.AcceptTcpClient();
			return new TcpReadConfirmationsStrategy(client, client.GetStream());
		}

		public void Dispose()
		{
		}
	}

	internal class UdpReadConfirmationsStrategyFactory : IReadConfirmationsStrategyFactory
	{
		private readonly System.Net.Sockets.UdpClient _udpConfirmationServer;

		public UdpReadConfirmationsStrategyFactory(System.Net.Sockets.UdpClient udpConfirmationServer)
		{
			_udpConfirmationServer = udpConfirmationServer;
		}

		public IReadConfirmationsStrategy CreateStrategy()
		{
			return new UdpReadConfirmationsStrategy(_udpConfirmationServer);
		}

		public void Dispose()
		{
			_udpConfirmationServer?.Close();
		}
	}
}
