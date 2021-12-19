using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Servers
{
	public class UdpServer : Server
	{
		protected readonly UdpClient server;

		public UdpServer(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
			if (!IPAddress.TryParse(serverAddress, out var iPAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}

			server = new UdpClient(new IPEndPoint(iPAddress, serverPort));
		}

		public override void Process()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var fileInfo = GetFileInfo();
			SaveFile(fileInfo);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			server.Close();
			Console.WriteLine("Выключение сервера");
		}

		protected WebProtocolsModel.FileInfo GetFileInfo()
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

			var data = server.Receive(ref remoteEndPoint);

			Console.WriteLine();
			Console.WriteLine("Пришел запрос на сервер");

			MemoryStream memory = new MemoryStream();
			memory.Write(data, 0, data.Length);
			memory.Position = 0;

			var binaryFormatter = new BinaryFormatter();

			var fileInfo = (WebProtocolsModel.FileInfo)binaryFormatter.Deserialize(memory);
			fileInfo.FileName = fileInfo.FileName.Replace(oldValue: "Client", newValue: "Server");

			return fileInfo;
		}

		private void SaveFile(WebProtocolsModel.FileInfo fileInfo)
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

			using (var binaryWritter = new BinaryWriter(new FileStream(fileInfo.FileName, FileMode.Create)))
			{
				byte[] buffer = new byte[bufferSize];

				for (int i = 0; i < fileInfo.FileSize; i += bufferSize)
				{
					var data = server.Receive(ref remoteEndPoint);
					binaryWritter.Write(data);
				}
			}
		}
	}
}
