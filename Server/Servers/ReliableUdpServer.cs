using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using WebProtocolsModel;

namespace Server.Servers
{
	internal class ReliableUdpServer : UdpServer
	{
		internal ReliableUdpServer(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
		}

		public override void Process()
		{
			var fileMetadata = GetFileMetadata();

			var confirmingClient = new TcpClient();
			confirmingClient.Connect(ServerIpAddress, ServerContext.ConfirmationPort);
			
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			SaveFile(fileMetadata, confirmingClient.GetStream());

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			Server.Close();
			confirmingClient.Close();
			Console.WriteLine("Выключение сервера");
		}

		private void SaveFile(FileMetadata fileMetadata, NetworkStream confirmingNetworkStream)
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
			var fileBatches = new Dictionary<int, FileBatch>();

			using (var binaryWriter = new BinaryWriter(new FileStream(fileMetadata.FileName, FileMode.Create)))
			{
				for (var i = 0; i < fileMetadata.FileSize; i += BufferSize)
				{
					var data = Server.Receive(ref remoteEndPoint);

					var fileBatch = GetFileBatch(data);
					fileBatches.Add(fileBatch.Order, fileBatch);

					ConfirmReceiving(fileBatch, confirmingNetworkStream);
				}

				binaryWriter.Write(
					fileBatches
						.OrderBy(b => b.Key)
						.SelectMany(b => b.Value.Bytes)
						.ToArray());
			}

		}

		private static FileBatch GetFileBatch(byte[] data)
		{
			var memory = new MemoryStream();
			memory.Write(data, 0, data.Length);
			memory.Position = 0;

			return new BinaryFormatter().Deserialize(memory) as FileBatch;
		}

		private static void ConfirmReceiving(FileBatch fileBatch, NetworkStream confirmingNetworkStream)
		{
			var binaryFormatter = new BinaryFormatter();

			var stream = new MemoryStream();
			binaryFormatter.Serialize(stream, new ConfirmMessage { BatchOrder = fileBatch.Order } );
			stream.Position = 0;

			var bytes = new byte[stream.Length];
			_ = stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

			confirmingNetworkStream.Write(bytes, 0, bytes.Length);
		}
	}
}
