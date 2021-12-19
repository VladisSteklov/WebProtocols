using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using WebProtocolsModel;

namespace Server.Servers
{
	public class StreamSocketServer : Server
	{
		private readonly Socket socket;

		private readonly IPEndPoint ipPoint;

		public StreamSocketServer(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
			if (!IPAddress.TryParse(serverAddress, out var remoteIPAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}

			ipPoint = new IPEndPoint(remoteIPAddress, serverPort);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public override void Process()
		{
			Console.WriteLine();
			Console.WriteLine("Старт сервера");

			socket.Bind(ipPoint);
			socket.Listen(1);

			var handler = socket.Accept();

			Console.WriteLine();
			Console.WriteLine("Пришел запрос на сервер");

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var fileInfo = GetFileInfo(handler);
			SaveFile(handler, fileInfo);
			//SaveFileWithDecompress(handler, fileInfo);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			handler.Shutdown(SocketShutdown.Receive);
			handler.Close();

			Console.WriteLine("Выключение сервера");
		}

		private WebProtocolsModel.FileInfo GetFileInfo(Socket handler)
		{
			var binaryFormatter = new BinaryFormatter();

			byte[] buffer = new byte[bufferSize];
			handler.Receive(buffer);

			var stream = new MemoryStream(buffer);
			stream.Position = 0;

			var fileInfo = (WebProtocolsModel.FileInfo)binaryFormatter.Deserialize(stream);
			fileInfo.FileName = fileInfo.FileName.Replace(oldValue: "Client", newValue: "Server");

			return fileInfo;
		}

		private void SaveFile(Socket handler, WebProtocolsModel.FileInfo fileInfo)
		{
			using (var binaryWritter = new BinaryWriter(new FileStream(fileInfo.FileName, FileMode.Create)))
			{
				byte[] buffer = new byte[bufferSize];

				for (int i = 0; i < fileInfo.FileSize; i += bufferSize)
				{
					handler.Receive(buffer);
					binaryWritter.Write(buffer);
				}
			}
		}

		private void SaveFileWithDecompress(Socket handler, WebProtocolsModel.FileInfo fileInfo)
		{
			using (var binaryWritter = new BinaryWriter(new MemoryStream()))
			{
				byte[] buffer = new byte[bufferSize];

				for (int i = 0; i < fileInfo.FileSize; i += bufferSize)
				{
					handler.Receive(buffer);
					binaryWritter.Write(buffer);
				}

				ImageCompressor.SaveAndDecompressDeflate(fileInfo.FileName, binaryWritter.BaseStream as MemoryStream);
			}
		}
	}
}
