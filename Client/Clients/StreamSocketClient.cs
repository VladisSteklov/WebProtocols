using System;
using System.IO;
using System.Net.Sockets;

using WebProtocolsModel;
using ProtocolType = System.Net.Sockets.ProtocolType;

namespace Client.Clients
{
	internal class StreamSocketClient : Client, IClient
	{
		private readonly Socket _socket;

		internal StreamSocketClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public void SendFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(fileName);
			}

			Console.Write("Подключение к удаленной точке\n");
			_socket.Connect(ServerIpEndPoint);

			using (var inputFileStream = new FileStream(fileName, FileMode.Open))
			{
				SendFileMetadata(inputFileStream);
				SendFile(inputFileStream);
				//SendFileWithCompress(inputFileStream);
			}

			_socket.Shutdown(SocketShutdown.Send);
			_socket.Close();
		}

		private void SendFileMetadata(FileStream fileStream)
		{
			var fileInfo = new FileMetadata
			{
				FileName = fileStream.Name,
				FileSize = fileStream.Length
			};

			_socket.Send(fileInfo.ToByteArray());
		}

		private void SendFile(Stream fileStream)
		{
			using var binaryReader = new BinaryReader(fileStream);
			var buffer = new byte[BufferSize];

			while (binaryReader.Read(buffer, 0, BufferSize) > 0)
			{
				_socket.Send(buffer);
			}
		}

		private void SendFileWithCompress(FileStream fileStream)
		{
			var compressedStream = ImageCompressor.CompressDeflate(fileStream);
			compressedStream.Position = 0;

			using var binaryReader = new BinaryReader(compressedStream);
			var buffer = new byte[BufferSize];

			while (binaryReader.Read(buffer, 0, BufferSize) > 0)
			{
				_socket.Send(buffer);
			}
		}

		public override void Dispose()
		{
			_socket?.Close();
		}

	}
}
