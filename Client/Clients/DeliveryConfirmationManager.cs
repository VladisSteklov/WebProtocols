using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using WebProtocolsModel;

namespace Client.Clients
{
	internal sealed class DeliveryConfirmationManager : IDisposable
	{
		private static readonly TimeSpan RetryTimeSpan = TimeSpan.FromSeconds(5);

		private class BatchConfirmationInfo
		{
			public bool IsConfirmed { get; set; }

			public DateTime LastRetryDateTimeUtc { get; set; }

			public FileBatch FileBatch { get; set; }
		}

		private readonly IReadOnlyDictionary<int, BatchConfirmationInfo> _fileBatches;

		private readonly TcpListener _deliveryConfirmationTcpListener;

		private readonly System.Net.Sockets.UdpClient _retrySendingUdpClient;

		private readonly IPEndPoint _serverIpEndPoint;

		private int _retryCounter;

		private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

		private Thread _readConfirmationsThread;

		public DeliveryConfirmationManager(
			System.Net.Sockets.UdpClient retrySendingUdpClient,
			IPEndPoint serverIpEndPoint,
			IDictionary<int, FileBatch> fileBatchesToConfirm)
		{
			_retrySendingUdpClient = retrySendingUdpClient;
			_serverIpEndPoint = serverIpEndPoint;

			_deliveryConfirmationTcpListener = new TcpListener(serverIpEndPoint.Address, ServerContext.ConfirmationPort);
			_fileBatches = new Dictionary<int, BatchConfirmationInfo>();
			_retryCounter = 0;

			_fileBatches = fileBatchesToConfirm
				.ToDictionary(
					b => b.Key,
					b =>
						new BatchConfirmationInfo
						{
							FileBatch = b.Value,
							IsConfirmed = false,
							LastRetryDateTimeUtc = DateTime.UtcNow
						});
		}

		public void StartConfirmation()
		{
			_deliveryConfirmationTcpListener.Start();

			_readConfirmationsThread = new Thread(ReadConfirmations);
			_readConfirmationsThread.Start();
		}

		public void StopConfirmation()
		{
			_readConfirmationsThread.Join();
			_deliveryConfirmationTcpListener.Stop();

			Console.WriteLine("Количество ретраев UDP" + _retryCounter);
			Console.WriteLine("Количество батчев" + _fileBatches.Count);
		}

		private void ReadConfirmations()
		{
			var confirmationsCounter = 0;

			var client = _deliveryConfirmationTcpListener.AcceptTcpClient();
			var networkStream = client.GetStream();

			while (confirmationsCounter < _fileBatches.Count)
			{
				if (networkStream.DataAvailable)
				{
					ConfirmBatch();
				}
				else
				{
					RetrySendingBatches();
				}
			}

			void ConfirmBatch()
			{
				if (!(BinaryFormatter.Deserialize(networkStream) is ConfirmMessage confirmMessage))
					throw new InvalidCastException($"Message from network stream is not {nameof(ConfirmMessage)}");

				_fileBatches[confirmMessage.BatchOrder].IsConfirmed = true;
				confirmationsCounter++;
			}
		}

		private void RetrySendingBatches()
		{
			var unconfirmedBatches = _fileBatches.Values.Where(b => !b.IsConfirmed);

			foreach (var batchInfo in unconfirmedBatches)
			{
				if (DateTime.UtcNow - batchInfo.LastRetryDateTimeUtc > RetryTimeSpan)
				{
					_retryCounter++;
					RetrySendingBatch(batchInfo, _serverIpEndPoint);
				}
			}
		}

		private void RetrySendingBatch(BatchConfirmationInfo batchConfirmationInfo, IPEndPoint iPEndPoint)
		{
			var sendingBytes = batchConfirmationInfo.FileBatch.ToByteArray();
			batchConfirmationInfo.LastRetryDateTimeUtc = DateTime.UtcNow;
			_retrySendingUdpClient.Send(sendingBytes, sendingBytes.Length, iPEndPoint);
		}

		public void Dispose()
		{
			_retrySendingUdpClient?.Dispose();
		}
	}
}
