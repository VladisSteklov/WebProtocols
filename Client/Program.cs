using System;
using System.Diagnostics;

using Client.Clients;

namespace Client
{
	internal class Program
	{
		private static readonly string fileName = "Example.jpg";

		static void Main()
		{
			Console.WriteLine("Программа клиента");

			using var client = SelectClient();

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
			do
			{
				Console.WriteLine();
				Console.WriteLine("Выберете тип используемого подключения");
				Console.WriteLine($"{ClientFactory.TcpClientKey} - TCP\n" +
				                  $"{ClientFactory.UdpClientKey} - UDP\n" +
				                  $"{ClientFactory.ReliableUdpClientTcpConfirmationKey} - Надежный UDP - подтверждение TCP\n" +
				                  $"{ClientFactory.ReliableUdpClientUdpConfirmationKey} - Надежный UDP - подтверждение UDP\n" +
								  $"{ClientFactory.StreamSocketClientKey} - Сокет TCP\n" +
				                  $"{ClientFactory.DgramSocketClientKey} - Сокет UDP");

				var key = Console.ReadKey().KeyChar;

				var client = ClientFactory.TryCreateClient(key);
				if (client != null)
					return client;

				Console.WriteLine("Ошибка при выборе клиента");
			}
			while (true);
		}
	}
}
