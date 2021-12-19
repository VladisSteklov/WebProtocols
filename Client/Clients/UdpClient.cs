using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Client.Clients
{
	public class UdpClient : Client
	{
		protected readonly System.Net.Sockets.UdpClient udpClient = new System.Net.Sockets.UdpClient();
		protected readonly IPEndPoint iPEndPoint;

		public UdpClient(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
			iPEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
		}

		public override void SendFile(string fileName)
		{
			using (var inputFileStream = new FileStream(fileName, FileMode.Open))
			{
				SendFileInfo(inputFileStream);
				SendFile(inputFileStream);
			}
			udpClient.Close();
		}

		protected void SendFileInfo(FileStream fileStream)
		{
			var binaryFormatter = new BinaryFormatter();
			var fileInfo = new WebProtocolsModel.FileInfo
			{
				FileName = fileStream.Name,
				FileSize = fileStream.Length
			};

			var stream = new MemoryStream();
			binaryFormatter.Serialize(stream, fileInfo);
			stream.Position = 0;

			var bytes = new byte[stream.Length];
			stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

			udpClient.Send(bytes, bytes.Length, iPEndPoint);
		}

		private void SendFile(FileStream fileStream)
		{
			using (var binaryReader = new BinaryReader(fileStream))
			{
				byte[] buffer = new byte[bufferSize];
				int count;

				while ((count = binaryReader.Read(buffer, 0, bufferSize)) > 0)
				{
					udpClient.Send(buffer, count, iPEndPoint);
				}
			}
		}
	}
}
