using Server.Servers;
using System;
namespace Server
{
	internal class Program
	{
		static void Main()
		{
			Console.WriteLine("Программа сервера");

			var server = SelectServer();
			server.Process();

			Console.ReadKey();
		}

		private static Servers.Server SelectServer()
		{
			do
			{
				Console.WriteLine();
				Console.WriteLine("Выберете тип используемого подключения");
				Console.WriteLine($"{ServerFactory.TcpServerKey} - TCP\n" +
				                  $"{ServerFactory.UdpServerKey} - UDP\n" +
				                  $"{ServerFactory.ReliableSlowUdpServerTcpConfirmServerKey} - Надежный медленный UDP - подтверждение TCP\n" +
				                  $"{ServerFactory.ReliableFastUdpServerTcpConfirmServerKey} - Надежный быстрый UDP - подтверждение TCP\n" +
				                  $"{ServerFactory.ReliableFastUdpServerUdpConfirmServerKey} - Надежный быстрый UDP - подтверждение UDP\n" +
								  $"{ServerFactory.StreamSocketServerKey} - Сокет TCP\n" +
				                  $"{ServerFactory.DgramSocketServerKey} - Сокет UDP");

				var key = Console.ReadKey().KeyChar;

				var server = ServerFactory.TryCreateServer(key);

				if (server != null)
					return server;

				Console.WriteLine("Неправильно указан ключ для выбора сервера");
			}
			while (true);
		}
	}
}
