using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Client.Clients;

using WebProtocolsModel;

namespace Client
{
	class Program
	{
		private static readonly string fileName = "Example.doc";

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
				Console.WriteLine("t - TCP\t u - UDP\t");

				key = Console.ReadKey().KeyChar;
				var serverContext = new ServerContext();

				switch (key)
				{
					case 't':
						return new TcpClient(serverContext.Address, serverContext.Port);
					case 'u':
						return new UdpClient(serverContext.Address, serverContext.Port);
					default:
						Console.WriteLine("Неправильно указан ключ операции");
						break;
				}

			}
			while (true);
		}
	}
}
