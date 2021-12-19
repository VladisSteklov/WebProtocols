using Server.Servers;
using System;
using System.Threading;
using System.Threading.Tasks;

using WebProtocolsModel;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Программа сервера");

			var server = SelectServer();
			server.Process();

			Console.ReadKey();
		}

		private static Servers.Server SelectServer()
		{
			char key;
			do
			{
				Console.WriteLine();
				Console.WriteLine("Выберете тип используемого подключения");
				Console.WriteLine("t - TCP\nu - UDP\nr - Надежный UDP\ns - Сокет TCP\nd - Сокет UDP");

				key = Console.ReadKey().KeyChar;

				switch (key)
				{
					case 't':
						return new TcpServer(ServerContext.Address, ServerContext.Port);
					case 'u':
						return new UdpServer(ServerContext.Address, ServerContext.Port);
					case 'r':
						return new ReliableUdpServer(ServerContext.Address, ServerContext.Port);
					case 's':
						return new StreamSocketServer(ServerContext.Address, ServerContext.Port);
					case 'd':
						return new DgramSocketServer(ServerContext.Address, ServerContext.Port);
					default:
						Console.WriteLine("Неправильно указан ключ операции");
						break;
				}

			}
			while (true);
		}
	}
}
