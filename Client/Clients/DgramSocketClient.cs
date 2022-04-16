using System.IO;
using System.Net.Sockets
	;
using WebProtocolsModel;
using ProtocolType = System.Net.Sockets.ProtocolType;

namespace Client.Clients
{
	internal class DgramSocketClient : Client
	{
		private readonly Socket _socket;

		internal DgramSocketClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}
		
		public override void SendFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(fileName);
			}

			using (var inputFileStream = new FileStream(fileName, FileMode.Open))
			{
				SendFileMetadata(inputFileStream);
				SendFile(inputFileStream);
			}

			_socket.Shutdown(SocketShutdown.Send);
			_socket.Close();
		}

		public override void Dispose()
		{
			_socket?.Close();
		}

		private void SendFileMetadata(FileStream fileStream)
		{
			var fileInfo = new FileMetadata
			{
				FileName = fileStream.Name,
				FileSize = fileStream.Length
			};

			_socket.SendTo(fileInfo.ToByteArray(), ServerIpEndPoint);
		}

		private void SendFile(Stream fileStream)
		{
			using var binaryReader = new BinaryReader(fileStream);
			byte[] buffer = new byte[BufferSize];

			while (binaryReader.Read(buffer, 0, BufferSize) > 0)
			{
				_socket.SendTo(buffer, ServerIpEndPoint);
			}
		}
	}
}
