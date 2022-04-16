using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

using Client.Clients.DeliveryConfirmationManager;
using Client.Clients.ProtocolVolatileWebClient.ProtocolVolatileStrategy;

using WebProtocolsModel;

namespace Client.Clients.ProtocolVolatileWebClient;

public class ProtocolVolatileWebClient : UdpClient
{
	private const double MaxRetryCountForTestPartFraction = 0.5;
	private const double FileBatchesCountForTestPartFromFileFraction = 0.25;

	private readonly BinaryFormatter _binaryFormatter = new();
	
	public ProtocolVolatileWebClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
	{
	}

	public override void SendFile(string fileName)
	{
		using (var inputFileStream = new FileStream(fileName, FileMode.Open))
		{
			SendFile(inputFileStream);
		}

		InternalUdpClient?.Close();
	}

	private void SendFile(FileStream fileStream)
	{
		byte[] data;
		using (var binaryReader = new BinaryReader(fileStream))
		{
			data = binaryReader.ReadBytes(Convert.ToInt32(fileStream.Length));
		}

		var (fileBatchesForTestPart, fileBatchesForMainPart)  = GetFileBatches(data);
		SendFileMetadata(fileStream, fileBatchesForTestPart.Count, fileBatchesForMainPart.Count);
		
		var retryCount = SendTestPart(fileBatchesForTestPart);
		
		using var protocolVolatileStrategy = ResolveProtocolForMainPart(retryCount, fileBatchesForTestPart.Count, fileBatchesForMainPart);
		SendMessageCore(protocolVolatileStrategy.ProtocolTypeMessage);
		protocolVolatileStrategy.SendFileForMainPart(fileBatchesForMainPart);
	}
	
	private void SendFileMetadata(FileStream fileStream, int fileBatchesForTestPartCount, int fileBatchesForMainPartCount)
	{
		var fileInfo = new FileBatchesMetadataMessage
		{
			FileName = fileStream.Name,
			FileBatchesForMainPartCount = fileBatchesForMainPartCount,
			FileBatchesForTestPartCount = fileBatchesForTestPartCount
		};

		SendMessageCore(fileInfo);
	}

	private void SendMessageCore(object message)
	{
		var stream = new MemoryStream();
		_binaryFormatter.Serialize(stream, message);
		stream.Position = 0;

		var bytes = new byte[stream.Length];
		_ = stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

		InternalUdpClient.Send(bytes, bytes.Length, ServerIpEndPoint);
	}
	
	
	private int SendTestPart(IReadOnlyDictionary<int, FileBatch> fileBatches)
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

	private IProtocolSendingMainPartStrategy ResolveProtocolForMainPart(
		int retryCount,
		int fileBatchesForTestPartCount,
		IReadOnlyDictionary<int, FileBatch> fileBatchesForMainPart)
	{
		var limitForUdpSending = fileBatchesForTestPartCount * MaxRetryCountForTestPartFraction;
		Console.WriteLine($"Порог для дальнейше UDP отправки {limitForUdpSending}");

		return retryCount < limitForUdpSending
			? new UdpProtocolSendingMainPartStrategy(InternalUdpClient, ServerIpEndPoint, CreateDeliveryConfirmationHost(fileBatchesForMainPart))
			: new TcpProtocolSendingMainPartStrategy(ServerIpEndPoint);
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

		var fileBatchesForTestPartCount = (int)Math.Ceiling(batches.Count * FileBatchesCountForTestPartFromFileFraction);
		var fileBatchesForTestPart = batches.Take(fileBatchesForTestPartCount).ToDictionary(p => p.Key, p => p.Value);

		var fileBatchesForMainPart = batches
			.Skip(fileBatchesForTestPartCount)
			.Take(batches.Count - fileBatchesForTestPartCount)
			.ToDictionary(p => p.Key, p => p.Value);

		return (fileBatchesForTestPart, fileBatchesForMainPart);
	}
}