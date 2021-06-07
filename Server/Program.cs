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
				Console.WriteLine("t - TCP\t u - UDP\t");

				key = Console.ReadKey().KeyChar;
				var serverContext = new ServerContext();

				switch (key)
				{
					case 't':
						return new TcpServer(serverContext.Address, serverContext.Port);
					case 'u':
						return new UdpServer(serverContext.Address, serverContext.Port);
					default:
						Console.WriteLine("Неправильно указан ключ операции");
						break;
				}

			}
			while (true);
		}
	}
}
