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
				                  $"{ServerFactory.UdpServer} - UDP\n" +
				                  $"{ServerFactory.ReliableUdpServer} - Надежный UDP\n" +
				                  $"{ServerFactory.StreamSocketServer} - Сокет TCP\n" +
				                  $"{ServerFactory.DgramSocketServer} - Сокет UDP");

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
