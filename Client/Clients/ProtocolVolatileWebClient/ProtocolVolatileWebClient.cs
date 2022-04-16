using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Client.Clients.DeliveryConfirmationManager;
using Client.Clients.ProtocolVolatileWebClient.ProtocolVolatileStrategy;
using WebProtocolsModel;

namespace Client.Clients.ProtocolVolatileWebClient;

public class ProtocolVolatileWebClient : UdpClient
{
	private const double MaxRetryCountFraction = 0.5;
	private const double CountFileBatchesForTestPartFraction = 0.25;

	public ProtocolVolatileWebClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
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

	private void SendFile(FileStream fileStream)
	{
		byte[] data;
		using (var binaryReader = new BinaryReader(fileStream))
		{
			data = binaryReader.ReadBytes(Convert.ToInt32(fileStream.Length));
		}

		var (fileBatchesForTestPart, fileBatchesForMainPart)  = GetFileBatches(data);

		var retryCount = SendTestPart(fileStream, fileBatchesForTestPart);
		
		var protocolVolatileStrategy = ResolveProtocol(retryCount);
		protocolVolatileStrategy.SendProtocolTypeMessage();
		protocolVolatileStrategy.SendFile();
	}

	private int SendTestPart(FileStream fileStream, IReadOnlyDictionary<int, FileBatch> fileBatches)
	{
		using var confirmationHost = CreateDeliveryConfirmationHost(fileBatches);
		var hostTask = confirmationHost.RunHostAsync();
		
		foreach (var sendingBytes in fileBatches.Values.Select(batch => batch.ToByteArray()))
		{
			InternalUdpClient.Send(sendingBytes.ToArray(), sendingBytes.Length, ServerIpEndPoint);
		}

		return hostTask.GetAwaiter().GetResult();
	}

	private DeliveryConfirmationHost CreateDeliveryConfirmationHost(IReadOnlyDictionary<int, FileBatch> fileBatches)
	{
		var udpConfirmationServer = new System.Net.Sockets.UdpClient(
			new IPEndPoint(ServerIpEndPoint.Address, ServerContext.ConfirmationPort));

		var confirmationsStrategyFactory = new UdpReadConfirmationsStrategyFactory(udpConfirmationServer); 
		return new DeliveryConfirmationHost(InternalUdpClient, confirmationsStrategyFactory, fileBatches, ServerIpEndPoint);
	}

	private static IProtocolVolatileStrategy ResolveProtocol(int retryCount)
	{
		return retryCount < _maxRetryCount ? new UdpProtocolVolatileStrategy() : new TcpProtocolVolatileStrategy();
		return new ProtocolVolatileResolver(retryCount).ResolveProtocol(retryCount);
	}

	private (Dictionary<int, FileBatch>, Dictionary<int, FileBatch>) GetFileBatches(IReadOnlyCollection<byte> data)
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

		var fileBatchesForTestPartCount = (int)Math.Ceiling(batches.Count * CountFileBatchesForTestPartFraction);
		var fileBatchesForTestPart = batches.Take(fileBatchesForTestPartCount).ToDictionary(p => p.Key, p => p.Value);

		var fileBatchesForMainPart = batches
			.Skip(fileBatchesForTestPartCount)
			.Take(batches.Count - fileBatchesForTestPartCount)
			.ToDictionary(p => p.Key, p => p.Value);

		return (fileBatchesForTestPart, fileBatchesForMainPart);
	}

	public override void Dispose()
	{
	}
}