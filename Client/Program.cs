using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Client.Clients;

using WebProtocolsModel;

namespace Client
{
	class Program
	{
		private static readonly string fileName = "ExampleEn.jpg";

		static void Main(string[] args)
		{
			Console.WriteLine("Программа клиента");

			var client = SelectClient();

			char key;
			do
			{
				Console.WriteLine();
				Console.WriteLine("Передать файл - t\t Завершить программу - q");
				key = Console.ReadKey().KeyChar;

				if (key == 't')
				{
					TransmitFile(client);
				}
			}
			while (key != 'q');

			Console.ReadKey();
		}

		private static void TransmitFile(Clients.Client client)
		{
			var stopwatch = new Stopwatch();

			Console.WriteLine("Начало передачи файла");
			stopwatch.Start();

			client.SendFile(fileName);

			stopwatch.Stop();
			Console.WriteLine("Конец передачи файла");
			Console.WriteLine($"Время передачи файла со стороны клиента состовляет {stopwatch.ElapsedMilliseconds} милисекунд");
		}

		private static Clients.Client SelectClient()
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
						return new TcpClient(ServerContext.Address, ServerContext.Port);
					case 'u':
						return new UdpClient(ServerContext.Address, ServerContext.Port);
					case 'r':
						return new ReliableUdpClient(ServerContext.Address, ServerContext.Port);
					case 's':
						return new StreamSocketClient(ServerContext.Address, ServerContext.Port);
					case 'd':
						return new DgramSocketClient(ServerContext.Address, ServerContext.Port);
					default:
						Console.WriteLine("Неправильно указан ключ операции");
						break;
				}

			}
			while (true);
		}
	}
}
