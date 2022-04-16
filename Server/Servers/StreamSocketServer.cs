using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;
using ProtocolType = System.Net.Sockets.ProtocolType;

namespace Server.Servers
{
	internal class StreamSocketServer : Server
	{
		private readonly Socket _socket;

		private readonly IPEndPoint _ipEndPoint;

		internal StreamSocketServer(string serverAddress, int serverPort) : base(serverAddress)
		{
			_ipEndPoint = new IPEndPoint(ServerIpAddress, serverPort);
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public override void Process()
		{
			Console.WriteLine();
			Console.WriteLine("Старт сервера");

			_socket.Bind(_ipEndPoint);
			_socket.Listen(backlog: 1);

			var handler = _socket.Accept();

			Console.WriteLine();
			Console.WriteLine("Пришел запрос на сервер");

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var fileInfo = GetFileMetadata(handler);
			SaveFile(handler, fileInfo);
			//SaveFileWithDecompress(handler, fileInfo);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			handler.Shutdown(SocketShutdown.Receive);
			handler.Close();

			Console.WriteLine("Выключение сервера");
		}

		private FileMetadata GetFileMetadata(Socket handler)
		{
			var binaryFormatter = new BinaryFormatter();

			var buffer = new byte[BufferSize];
			handler.Receive(buffer);

			var stream = new MemoryStream(buffer)
			{
				Position = 0
			};

			var fileInfo = (FileMetadata)binaryFormatter.Deserialize(stream);
			fileInfo.FileName = fileInfo.FileName.Replace(oldValue: "Client", newValue: "Server");

			return fileInfo;
		}

		private void SaveFile(Socket handler, FileMetadata fileMetadata)
		{
			using var binaryWriter = new BinaryWriter(new FileStream(fileMetadata.FileName, FileMode.Create));
			var buffer = new byte[BufferSize];
			for (var i = 0; i < fileMetadata.FileSize; i += BufferSize)
			{
				handler.Receive(buffer);
				binaryWriter.Write(buffer);
			}
		}

		private void SaveFileWithDecompress(Socket handler, WebProtocolsModel.FileMetadata fileMetadata)
		{
			using var binaryWriter = new BinaryWriter(new MemoryStream());
			byte[] buffer = new byte[BufferSize];

			for (int i = 0; i < fileMetadata.FileSize; i += BufferSize)
			{
				handler.Receive(buffer);
				binaryWriter.Write(buffer);
			}

			ImageCompressor.SaveAndDecompressDeflate(fileMetadata.FileName, binaryWriter.BaseStream as MemoryStream);
		}
	}
}
