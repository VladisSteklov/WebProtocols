using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using WebProtocolsModel;

namespace Client.Clients
{
	internal abstract class ReliableUdpClient : UdpClient
	{
		internal ReliableUdpClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
		{
		}

		public sealed override void SendFile(string fileName)
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

			using var _ = CreateDeliveryConfirmationManager(fileBatches);

			foreach (var sendingBytes in fileBatches.Values.Select(batch => batch.ToByteArray()))
			{
				InternalUdpClient.Send(sendingBytes.ToArray(), sendingBytes.Length, ServerIpEndPoint);
			}
		}

		private DeliveryConfirmationManager.DeliveryConfirmationManager CreateDeliveryConfirmationManager(
			IReadOnlyDictionary<int, FileBatch> fileBatches) =>
			new(InternalUdpClient, CreateConfirmationsStrategyFactory(), fileBatches, ServerIpEndPoint);

		protected abstract DeliveryConfirmationManager.IReadConfirmationsStrategyFactory CreateConfirmationsStrategyFactory();

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
