using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using WebProtocolsModel;

namespace Server.Servers
{
	public class TcpServer : Server
	{
		private readonly TcpListener server;

		public TcpServer(string serverAddress, int serverPort)
		{
			if (!IPAddress.TryParse(serverAddress, out var iPAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}

			server = new TcpListener(iPAddress, serverPort);
		}

		public override void Process()
		{
			Console.WriteLine();
			Console.WriteLine("Старт сервера");
			server.Start();

			var client = server.AcceptTcpClient();
			Console.WriteLine();
			Console.WriteLine("Пришел запрос на сервер");

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var networkStream = client.GetStream();

			var fileInfo = GetFileInfo(networkStream);
			SaveFile(networkStream, fileInfo);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			server.Stop();
			Console.WriteLine("Выключение сервера");
		}

		private WebProtocolsModel.FileInfo GetFileInfo(NetworkStream networkStream)
		{
			var binaryFormatter = new BinaryFormatter();

			var fileInfo = (WebProtocolsModel.FileInfo)binaryFormatter.Deserialize(networkStream);
			fileInfo.FileName = fileInfo.FileName.Replace(oldValue: "Client", newValue: "Server");

			return fileInfo;
		}

		private void SaveFile(NetworkStream networkStream, WebProtocolsModel.FileInfo fileInfo)
		{
			using (var binaryWritter = new BinaryWriter(new FileStream(fileInfo.FileName, FileMode.Create)))
			{
				byte[] buffer = new byte[bufferSize];

				for (int i = 0; i < fileInfo.FileSize; i += bufferSize)
				{
					networkStream.Read(buffer, 0, bufferSize);
					binaryWritter.Write(buffer);
				}
			}
		}
	}
}
