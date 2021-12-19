using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WebProtocolsModel;

namespace Client.Clients
{
	public class DgramSocketClient : Client
	{
		private readonly Socket socket;

		private readonly IPEndPoint remoteIpPoint;

		public DgramSocketClient(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
			if (!IPAddress.TryParse(serverAddress, out var remoteIPAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}

			remoteIpPoint = new IPEndPoint(remoteIPAddress, serverPort);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}
		
		public override void SendFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(fileName);
			}

			using (var inputFileStream = new FileStream(fileName, FileMode.Open))
			{
				SendFileInfo(inputFileStream);
				SendFile(inputFileStream);
			}

			socket.Shutdown(SocketShutdown.Send);
			socket.Close();
		}

		private void SendFileInfo(FileStream fileStream)
		{
			var fileInfo = new WebProtocolsModel.FileInfo
			{
				FileName = fileStream.Name,
				FileSize = fileStream.Length
			};

			var bytes = SendingDataHelper.PrepareData(fileInfo);

			socket.SendTo(bytes, remoteIpPoint);
		}

		private void SendFile(FileStream fileStream)
		{
			using (var binaryReader = new BinaryReader(fileStream))
			{
				byte[] buffer = new byte[bufferSize];

				while (binaryReader.Read(buffer, 0, bufferSize) > 0)
				{
					socket.SendTo(buffer, remoteIpPoint);
				}
			}
		}
	}
}
