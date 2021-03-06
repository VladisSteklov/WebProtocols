using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;

namespace Client.Clients
{
	internal sealed class TcpClient : Client, IClient
	{
		private readonly System.Net.Sockets.TcpClient _tcpClient;

		internal TcpClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
		{
			_tcpClient = new System.Net.Sockets.TcpClient();
		}

		public void SendFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(fileName);
			}

			_tcpClient.Connect(ServerIpEndPoint);

			using (var networkStream = _tcpClient.GetStream())
			{
				using (var inputFileStream = new FileStream(fileName, FileMode.Open))
				{
					SendFileMetadata(networkStream, inputFileStream);
					SendFile(networkStream, inputFileStream);
				}
			}

			_tcpClient.Close();		
		}

		private static void SendFileMetadata(Stream networkStream, FileStream fileStream)
		{
			var binaryFormatter = new BinaryFormatter();
			var fileInfo = new FileMetadata
			{
				FileName = fileStream.Name,
				FileSize = fileStream.Length
			};

			binaryFormatter.Serialize(networkStream, fileInfo);
		}

		private void SendFile(Stream networkStream, FileStream fileStream)
		{
			using var binaryReader = new BinaryReader(fileStream);
			var buffer = new byte[BufferSize];
			int count;

			while ((count = binaryReader.Read(buffer, 0, BufferSize)) > 0)
			{
				networkStream.Write(buffer, 0, count);
			}
		}

		public override void Dispose()
		{
			_tcpClient?.Close();
		}
	}
}
