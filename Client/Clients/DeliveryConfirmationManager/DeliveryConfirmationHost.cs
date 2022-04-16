﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using WebProtocolsModel;

namespace Client.Clients.DeliveryConfirmationManager
{
	public class DeliveryConfirmationHost : IDisposable
	{
		private static readonly TimeSpan RetryTimeSpan = TimeSpan.FromMilliseconds(50);

		private class BatchConfirmationInfo
		{
			public bool IsConfirmed { get; set; }
			public DateTime LastRetryDateTimeUtc { get; set; }
			public FileBatch FileBatch { get; set; }
		}

		private readonly IReadOnlyDictionary<int, BatchConfirmationInfo> _fileBatches;
		private readonly System.Net.Sockets.UdpClient _retrySendingUdpClient;
		private readonly IReadConfirmationsStrategyFactory _readConfirmationsStrategyFactory;
		private readonly IPEndPoint _serverIpEndPoint;

		private int _retryCounter;

		public DeliveryConfirmationHost(
			System.Net.Sockets.UdpClient retrySendingUdpClient,
			IReadConfirmationsStrategyFactory readConfirmationsStrategyFactory,
			IReadOnlyDictionary<int, FileBatch> fileBatchesToConfirm,
			IPEndPoint serverIpEndPoint)
		{
			_retrySendingUdpClient = retrySendingUdpClient;
			_readConfirmationsStrategyFactory = readConfirmationsStrategyFactory;
			_serverIpEndPoint = serverIpEndPoint;

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

		public Task<int> RunHostAsync()
		{
			return Task.Run(ReadConfirmations);
		}
		
		
		private int ReadConfirmations()
		{
			var confirmationsCounter = 0;

			using var readConfirmationStrategy = _readConfirmationsStrategyFactory.CreateStrategy();

			while (confirmationsCounter < _fileBatches.Count)
			{
				var confirmMessage = readConfirmationStrategy.TryReadConfirmation();
				if (confirmMessage != null)
				{
					ConfirmBatch(confirmMessage);
				}
				else
				{
					RetrySendingBatches();
				}
			}

			return _retryCounter;

			void ConfirmBatch(ConfirmMessage confirmMessage)
			{
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
					RetrySendingBatch(batchInfo);
				}
			}
		}

		private void RetrySendingBatch(BatchConfirmationInfo batchConfirmationInfo)
		{
			var sendingBytes = batchConfirmationInfo.FileBatch.ToByteArray();

			batchConfirmationInfo.LastRetryDateTimeUtc = DateTime.UtcNow;
			_retrySendingUdpClient.Send(sendingBytes, sendingBytes.Length, _serverIpEndPoint);
		}

		public void Dispose()
		{
			Console.WriteLine("Количество ретраев UDP " + _retryCounter);
			Console.WriteLine("Количество батчев " + _fileBatches.Count);
		}
	}
}
