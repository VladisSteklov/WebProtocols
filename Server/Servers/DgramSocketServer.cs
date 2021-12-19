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

namespace Server.Servers
{
	public class DgramSocketServer : Server
	{
		private readonly Socket socket;

		private readonly IPEndPoint ipPoint;

		public DgramSocketServer(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
			if (!IPAddress.TryParse(serverAddress, out var remoteIPAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}

			ipPoint = new IPEndPoint(remoteIPAddress, serverPort);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}

		public override void Process()
		{
			Console.WriteLine();
			Console.WriteLine("Старт сервера");
			Console.WriteLine();

			socket.Bind(ipPoint);

			EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
			var fileInfo = GetFileInfo(ref remoteIp);

			Console.WriteLine("Пришел запрос на сервер");
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			SaveFile(fileInfo, ref remoteIp);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			socket.Shutdown(SocketShutdown.Receive);
			socket.Close();

			Console.WriteLine("Выключение сервера");
		}

		private WebProtocolsModel.FileInfo GetFileInfo(ref EndPoint remoteIp)
		{
			var binaryFormatter = new BinaryFormatter();

			byte[] buffer = new byte[bufferSize];

			socket.ReceiveFrom(buffer, ref remoteIp);

			var stream = new MemoryStream(buffer);
			stream.Position = 0;

			var fileInfo = (WebProtocolsModel.FileInfo)binaryFormatter.Deserialize(stream);
			fileInfo.FileName = fileInfo.FileName.Replace(oldValue: "Client", newValue: "Server");

			return fileInfo;
		}

		private void SaveFile(WebProtocolsModel.FileInfo fileInfo, ref EndPoint remoteIp)
		{
			using (var binaryWritter = new BinaryWriter(new FileStream(fileInfo.FileName, FileMode.Create)))
			{
				byte[] buffer = new byte[bufferSize];

				for (int i = 0; i < fileInfo.FileSize; i += bufferSize)
				{
					socket.ReceiveFrom(buffer, ref remoteIp);
					binaryWritter.Write(buffer);
				}
			}
		}
	}
}
