using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Client.Clients
{
	class TcpClient : Client
	{
		private readonly System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();

		public TcpClient(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
		}

		public override void SendFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(fileName);
			}

			tcpClient.Connect(serverAddress, serverPort);
			var networkStream = tcpClient.GetStream();

			using (var inputFileStream = new FileStream(fileName, FileMode.Open))
			{
				SendFileInfo(networkStream, inputFileStream);
				SendFile(networkStream, inputFileStream);
			}

			tcpClient.Close();		
		}

		private void SendFileInfo(NetworkStream networkStream, FileStream fileStream)
		{
			var binaryFormatter = new BinaryFormatter();
			var fileInfo = new WebProtocolsModel.FileInfo
			{
				FileName = fileStream.Name,
				FileSize = fileStream.Length
			};

			binaryFormatter.Serialize(networkStream, fileInfo);
		}

		private void SendFile(NetworkStream networkStream, FileStream fileStream)
		{
			using (var binaryReader = new BinaryReader(fileStream))
			{
				byte[] buffer = new byte[bufferSize];
				int count;

				while ((count = binaryReader.Read(buffer, 0, bufferSize)) > 0)
				{
					networkStream.Write(buffer, 0, count);
				}
			}
		}
	}
}
