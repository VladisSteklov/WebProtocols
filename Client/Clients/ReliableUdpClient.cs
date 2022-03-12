using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using WebProtocolsModel;

namespace Client.Clients
{
	internal class ReliableUdpClient : UdpClient
	{
		internal ReliableUdpClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
		{
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

		private void SendFile(Stream fileStream)
		{
			byte[] data;
			using (var binaryReader = new BinaryReader(fileStream))
			{
				data = binaryReader.ReadBytes(Convert.ToInt32(fileStream.Length));
			}

			var fileBatches = GetFileBatches(data);

			var deliveryConfirmationManager = new DeliveryConfirmationManager(
				InternalUdpClient,
				ServerIpEndPoint,
				fileBatches);

			deliveryConfirmationManager.StartConfirmation();
			foreach (var sendingBytes in fileBatches.Values.Select(batch => batch.ToByteArray()))
			{
				InternalUdpClient.Send(sendingBytes.ToArray(), sendingBytes.Length, ServerIpEndPoint);
			}

			deliveryConfirmationManager.StopConfirmation();
		}

		private Dictionary<int, FileBatch> GetFileBatches(IReadOnlyCollection<byte> data)
		{
			var batches = new Dictionary<int, FileBatch>();

			for (var i = 0; i < data.Count; i += BufferSize)
			{
				var index = i / BufferSize;
				batches.Add(
					index,
					new FileBatch
					{
						Order = index,
						Bytes = data.Skip(index * BufferSize).Take(BufferSize).ToArray()
					});
			}

			return batches;
		}
	}
}
