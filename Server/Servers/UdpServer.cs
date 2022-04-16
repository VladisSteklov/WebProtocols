using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;

namespace Server.Servers
{
	public class UdpServer : Server
	{
		protected UdpClient Server { get; }

		internal UdpServer(string serverAddress, int serverPort) : base(serverAddress)
		{
			Server = new UdpClient(new IPEndPoint(ServerIpAddress, serverPort));
		}

		public override void Process()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var fileInfo = GetFileMetadata();
			SaveFile(fileInfo);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			Server.Close();
			Console.WriteLine("Выключение сервера");
		}

		protected FileMetadata GetFileMetadata()
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

			var data = Server.Receive(ref remoteEndPoint);

			Console.WriteLine();
			Console.WriteLine("Пришел запрос на сервер");

			var memory = new MemoryStream();
			memory.Write(data, 0, data.Length);
			memory.Position = 0;

			var binaryFormatter = new BinaryFormatter();

			var fileInfo = (FileMetadata)binaryFormatter.Deserialize(memory);
			fileInfo.FileName = fileInfo.FileName.Replace(oldValue: "Client", newValue: "Server");

			return fileInfo;
		}

		private void SaveFile(FileMetadata fileMetadata)
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

			using var binaryWriter = new BinaryWriter(new FileStream(fileMetadata.FileName, FileMode.Create));
			for (var i = 0; i < fileMetadata.FileSize; i += BufferSize)
			{
				var data = Server.Receive(ref remoteEndPoint);
				binaryWriter.Write(data);
			}
		}
	}
}
