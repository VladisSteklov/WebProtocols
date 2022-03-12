using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;

namespace Client.Clients
{
	internal class UdpClient : Client
	{
		protected System.Net.Sockets.UdpClient InternalUdpClient { get; }

		public UdpClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
		{
			InternalUdpClient = new System.Net.Sockets.UdpClient();
		}

		public override void SendFile(string fileName)
		{
			using (var inputFileStream = new FileStream(fileName, FileMode.Open))
			{
				SendFileMetadata(inputFileStream);
				SendFile(inputFileStream);
			}

			InternalUdpClient.Close();
		}

		
		protected void SendFileMetadata(FileStream fileStream)
		{
			var fileInfo = new FileMetadata
			{
				FileName = fileStream.Name,
				FileSize = fileStream.Length
			};

			var stream = new MemoryStream();
			new BinaryFormatter().Serialize(stream, fileInfo);
			stream.Position = 0;

			var bytes = new byte[stream.Length];
			_ = stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

			InternalUdpClient.Send(bytes, bytes.Length, ServerIpEndPoint);
		}

		private void SendFile(Stream fileStream)
		{
			using var binaryReader = new BinaryReader(fileStream);
			var buffer = new byte[BufferSize];
			int count;

			while ((count = binaryReader.Read(buffer, 0, BufferSize)) > 0)
			{
				InternalUdpClient.Send(buffer, count, ServerIpEndPoint);
			}
		}

		public override void Dispose()
		{
			InternalUdpClient?.Close();
		}
	}
}
