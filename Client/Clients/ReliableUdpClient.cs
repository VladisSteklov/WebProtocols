using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebProtocolsModel;

namespace Client.Clients
{
	public class ReliableUdpClient : UdpClient
	{
		private readonly IPAddress tcpConfirmationServerAddress;

		public ReliableUdpClient(string serverAddress, int serverPort) : base(serverAddress, serverPort)
		{
			if (!IPAddress.TryParse(serverAddress, out tcpConfirmationServerAddress))
			{
				throw new InvalidOperationException($"String {serverAddress} is not ip address");
			}
		}

		public override void SendFile(string fileName)
		{
			var confirmationManager = new ConfirmationManager(
				new TcpListener(tcpConfirmationServerAddress, ServerContext.ConfirmationPort),
				udpClient,
				iPEndPoint);

			using (var inputFileStream = new FileStream(fileName, FileMode.Open))
			{
				SendFileInfo(inputFileStream);

				confirmationManager.StartConfirmation();

				SendFileCore(inputFileStream, confirmationManager);
			}

			confirmationManager.StopConfirmation();
			udpClient.Close();
		}

		private void SendFileCore(FileStream fileStream, ConfirmationManager confirmationManager)
		{
			byte[] data;
			using (var binaryReader = new BinaryReader(fileStream))
			{
				data = binaryReader.ReadBytes(Convert.ToInt32(fileStream.Length));
			}

			var fileBatches = GetFileBatches(data);

			foreach (var batch in fileBatches.Values)
			{
				var sendingBytes = SendingDataHelper.PrepareData(batch);
				confirmationManager.AddBatch(batch);
				udpClient.Send(sendingBytes, sendingBytes.Length, iPEndPoint);
			}
		}

		private Dictionary<int, FileBatch> GetFileBatches(byte[] data)
		{
			var batches = new Dictionary<int, FileBatch>();

			for (var i = 0; i < data.Length; i += bufferSize)
			{
				var index = i / bufferSize;
				batches.Add(
					index,
					new FileBatch
					{
						Order = index,
						Bytes = data.Skip(index * bufferSize).Take(bufferSize).ToArray()
					});
			}

			return batches;
		}
	}
}
