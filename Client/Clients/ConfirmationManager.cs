using System;
using System.Collections.Generic;
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
	public class ConfirmationManager
	{
		private class BatchConfirmationInfo
		{
			public bool IsConfirmed { get; set; }

			public DateTime LastRetryDateTimeUtc { get; set; }

			public FileBatch FileBatch { get; set; }
		}

		private readonly IDictionary<int, BatchConfirmationInfo> fileBatches;

		private readonly TcpListener confirmingTcpListener;

		private readonly System.Net.Sockets.UdpClient sendingUdpClient;

		private readonly IPEndPoint iPEndPoint;

		private readonly TimeSpan retryTimeSpan = TimeSpan.FromSeconds(5);

		private Thread readConfirmationsThread;

		private int retryCounter;

		public ConfirmationManager(
			TcpListener confirmingTcpListener,
			System.Net.Sockets.UdpClient sendingUdpClient,
			IPEndPoint iPEndPoint)
		{
			this.confirmingTcpListener = confirmingTcpListener;
			this.sendingUdpClient = sendingUdpClient;
			this.iPEndPoint = iPEndPoint;

			fileBatches = new Dictionary<int, BatchConfirmationInfo>();
		}

		public void AddBatch(FileBatch fileBatch)
		{
			fileBatches.Add(
				fileBatch.Order,
				new BatchConfirmationInfo
				{
					FileBatch = fileBatch,
					IsConfirmed = false,
					LastRetryDateTimeUtc = DateTime.UtcNow
				});
		}

		public void StartConfirmation()
		{
			retryCounter = 0;
			confirmingTcpListener.Start();

			readConfirmationsThread = new Thread(() => ReadConfirmations());
			readConfirmationsThread.Start();
		}

		public void StopConfirmation()
		{
			readConfirmationsThread.Join();
			confirmingTcpListener.Stop();

			Console.WriteLine("Количество ретраев UDP" + retryCounter);
			Console.WriteLine("Количество батчев" + fileBatches.Count);
		}

		private void ReadConfirmations()
		{
			WaitBatches();

			var confirmationsCounter = 0;

			var client = confirmingTcpListener.AcceptTcpClient();
			var networkStream = client.GetStream();

			while (confirmationsCounter < fileBatches.Count)
			{
				var binaryFormatter = new BinaryFormatter();

				if (networkStream.DataAvailable)
				{
					var confirmMessage = binaryFormatter.Deserialize(networkStream) as ConfirmMessage;

					fileBatches[confirmMessage.BatchOrder].IsConfirmed = true;
					confirmationsCounter++;
				}
				else
				{
					RetrySendingBatches();
				}
			}
		}

		private void RetrySendingBatches()
		{
			var unconfirmedBatches = fileBatches.Values.Where(b => !b.IsConfirmed).ToArray();

			foreach (var batchInfo in unconfirmedBatches)
			{
				if (DateTime.UtcNow - batchInfo.LastRetryDateTimeUtc > retryTimeSpan)
				{
					retryCounter++;
					RetrySendingBatch(batchInfo, iPEndPoint);
				}
			}
		}

		private void RetrySendingBatch(BatchConfirmationInfo batchConfirmationInfo, IPEndPoint iPEndPoint)
		{
			var sendingBytes = SendingDataHelper.PrepareData(batchConfirmationInfo.FileBatch);
			batchConfirmationInfo.LastRetryDateTimeUtc = DateTime.UtcNow;
			sendingUdpClient.Send(sendingBytes, sendingBytes.Length, iPEndPoint);
		}

		private void WaitBatches()
		{
			while (fileBatches.Count == 0)
			{
				Thread.Sleep(1000);
			}
		}
	}
}
