using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;

namespace Server.Servers
{
	internal class DgramSocketServer : Server
	{
		private readonly Socket _socket;

		private readonly IPEndPoint _ipEndPoint;

		internal DgramSocketServer(string serverAddress, int serverPort) : base(serverAddress)
		{
			_ipEndPoint = new IPEndPoint(ServerIpAddress, serverPort);
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}

		public override void Process()
		{
			Console.WriteLine();
			Console.WriteLine("Старт сервера");
			Console.WriteLine();

			_socket.Bind(_ipEndPoint);

			EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
			var fileInfo = GetFileMetadata(ref remoteIp);

			Console.WriteLine("Пришел запрос на сервер");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			SaveFile(fileInfo, ref remoteIp);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			_socket.Shutdown(SocketShutdown.Receive);
			_socket.Close();

			Console.WriteLine("Выключение сервера");
		}

		private FileMetadata GetFileMetadata(ref EndPoint remoteIp)
		{
			var binaryFormatter = new BinaryFormatter();

			var buffer = new byte[BufferSize];

			_socket.ReceiveFrom(buffer, ref remoteIp);

			var stream = new MemoryStream(buffer)
			{
				Position = 0
			};

			var fileInfo = (FileMetadata)binaryFormatter.Deserialize(stream);
			fileInfo.FileName = fileInfo.FileName.Replace(oldValue: "Client", newValue: "Server");

			return fileInfo;
		}

		private void SaveFile(FileMetadata fileMetadata, ref EndPoint remoteIp)
		{
			using var binaryWriter = new BinaryWriter(new FileStream(fileMetadata.FileName, FileMode.Create));
			var buffer = new byte[BufferSize];

			for (var i = 0; i < fileMetadata.FileSize; i += BufferSize)
			{
				_socket.ReceiveFrom(buffer, ref remoteIp);
				binaryWriter.Write(buffer);
			}
		}
	}
}
