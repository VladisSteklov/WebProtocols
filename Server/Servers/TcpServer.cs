using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;

namespace Server.Servers
{
	internal class TcpServer : Server
	{
		private readonly TcpListener _server;

		internal TcpServer(string serverAddress, int serverPort) : base(serverAddress)
		{
			_server = new TcpListener(ServerIpAddress, serverPort);
		}

		public override void Process()
		{
			Console.WriteLine();
			Console.WriteLine("Старт сервера");

			_server.Start();
			var client = _server.AcceptTcpClient();

			Console.WriteLine();
			Console.WriteLine("Пришел запрос на сервер");

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var networkStream = client.GetStream();

			var fileInfo = GetFileMetadata(networkStream);
			SaveFile(networkStream, fileInfo);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			_server.Stop();
			Console.WriteLine("Выключение сервера");
		}

		private static FileMetadata GetFileMetadata(Stream networkStream)
		{
			var binaryFormatter = new BinaryFormatter();

			var fileInfo = (FileMetadata)binaryFormatter.Deserialize(networkStream);
			fileInfo.FileName = fileInfo.FileName.Replace(oldValue: "Client", newValue: "Server");

			return fileInfo;
		}

		private void SaveFile(NetworkStream networkStream, FileMetadata fileMetadata)
		{
			using var binaryWriter = new BinaryWriter(new FileStream(fileMetadata.FileName, FileMode.Create));
			var buffer = new byte[BufferSize];

			for (var i = 0; i < fileMetadata.FileSize; i += BufferSize)
			{
				_ = networkStream.Read(buffer, 0, BufferSize);
				binaryWriter.Write(buffer);
			}
		}
	}
}
