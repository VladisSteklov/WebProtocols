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
	public class ReliableUdpServer : UdpServer
	{
		public ReliableUdpServer(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
		}

		public override void Process()
		{
			var fileInfo = GetFileInfo();

			var confirmingClient = new TcpClient();
			confirmingClient.Connect(serverAddress, ServerContext.ConfirmationPort);
			var confirmingNetworkStream = confirmingClient.GetStream();

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			SaveFile(fileInfo, confirmingNetworkStream);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			server.Close();
			confirmingClient.Close();
			Console.WriteLine("Выключение сервера");
		}

		private void SaveFile(WebProtocolsModel.FileInfo fileInfo, NetworkStream confirmingNetworkStream)
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
			var fileBatches = new Dictionary<int, FileBatch>();

			using (var binaryWritter = new BinaryWriter(new FileStream(fileInfo.FileName, FileMode.Create)))
			{
				for (int i = 0; i < fileInfo.FileSize; i += bufferSize)
				{
					var data = server.Receive(ref remoteEndPoint);

					var fileBatch = GetFileBatch(data);
					fileBatches.Add(fileBatch.Order, fileBatch);

					ConfirmReceiving(fileBatch, confirmingNetworkStream, remoteEndPoint);
				}

				binaryWritter.Write(
					fileBatches
						.OrderBy(b => b.Key)
						.SelectMany(b => b.Value.Bytes)
						.ToArray());
			}

		}

		private FileBatch GetFileBatch(byte[] data)
		{
			var memory = new MemoryStream();
			memory.Write(data, 0, data.Length);
			memory.Position = 0;

			return new BinaryFormatter().Deserialize(memory) as FileBatch;
		}

		private void ConfirmReceiving(FileBatch fileBatch, NetworkStream confirmingNetworkStream, IPEndPoint iPEndPoint)
		{
			var binaryFormatter = new BinaryFormatter();

			var stream = new MemoryStream();
			binaryFormatter.Serialize(stream, new ConfirmMessage { BatchOrder = fileBatch.Order } );
			stream.Position = 0;

			var bytes = new byte[stream.Length];
			stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

			confirmingNetworkStream.Write(bytes, 0, bytes.Length);
		}
	}
}
