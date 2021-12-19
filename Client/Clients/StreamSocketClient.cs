using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

using WebProtocolsModel;

namespace Client.Clients
{
	public class StreamSocketClient : Client
	{
		private readonly Socket socket;

		private readonly IPEndPoint remoteIpPoint;

		public StreamSocketClient(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
			if (!IPAddress.TryParse(serverAddress, out var remoteIPAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}

			remoteIpPoint = new IPEndPoint(remoteIPAddress, serverPort);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public override void SendFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(fileName);
			}

			Console.Write("Подключение к удаленной точке\n");
			socket.Connect(remoteIpPoint);

			using (var inputFileStream = new FileStream(fileName, FileMode.Open))
			{
				SendFileInfo(inputFileStream);
				SendFile(inputFileStream);
				//SendFileWithCompress(inputFileStream);
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

			socket.Send(bytes);
		}

		private void SendFile(FileStream fileStream)
		{
			using (var binaryReader = new BinaryReader(fileStream))
			{
				byte[] buffer = new byte[bufferSize];

				while (binaryReader.Read(buffer, 0, bufferSize) > 0)
				{
					socket.Send(buffer);
				}
			}
		}

		private void SendFileWithCompress(FileStream fileStream)
		{
			var compressedStream = ImageCompressor.CompressDeflate(fileStream);
			compressedStream.Position = 0;

			using (var binaryReader = new BinaryReader(compressedStream))
			{
				byte[] buffer = new byte[bufferSize];

				while (binaryReader.Read(buffer, 0, bufferSize) > 0)
				{
					socket.Send(buffer);
				}
			}
		}
	}
}
