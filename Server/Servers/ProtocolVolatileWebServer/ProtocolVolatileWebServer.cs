using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Server.Servers.ConfirmStrategy;
using Server.Servers.ConfirmStrategy.ConfirmStrategyFactory;
using WebProtocolsModel;
using ProtocolType = WebProtocolsModel.ProtocolType;

namespace Server.Servers.ProtocolVolatileWebServer;

public class ProtocolVolatileWebServer : UdpServer
{
	private static readonly BinaryFormatter BinaryFormatter = new();
	
	private readonly IConfirmStrategy _confirmStrategy;
	private readonly int _serverPort;
	
	public ProtocolVolatileWebServer(string serverAddress, int serverPort) : base(serverAddress, serverPort)
	{
		_serverPort = serverPort;
		_confirmStrategy = new UdpConfirmStrategyFactory().CreateStrategy(ServerIpAddress);
	}
	
	public override void Process()
	{
		var fileMetadata = GetMessage<FileBatchesMetadataMessage>();
		ChangeFileName(fileMetadata);
		
		Console.WriteLine();
		Console.WriteLine("Пришел запрос на сервер");
		
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		var fileBatches = ReceiveFileBatches(fileMetadata);
		SaveFile(fileMetadata, fileBatches);

		stopwatch.Stop();
		Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

		Server.Close();
		Console.WriteLine("Выключение сервера");
	}

	private IDictionary<int, FileBatch> ReceiveFileBatches(FileBatchesMetadataMessage fileMetadata)
	{
		var fileBatchesFromTestPart = ReceiveFileBatches(fileMetadata.FileBatchesForTestPartCount);
		var protocolTypeMessage = GetMessage<ProtocolTypeMessage>();
		
		return fileBatchesFromTestPart
			.Concat(ReceiveMainPart(protocolTypeMessage, fileMetadata.FileBatchesForMainPartCount))
			.ToDictionary(p => p.Key, p => p.Value);
	}

	IDictionary<int, FileBatch> ReceiveMainPart(ProtocolTypeMessage protocolTypeMessage, int fileBatchesForMainPartCount)
	{
		return protocolTypeMessage.ProtocolType switch
		{
			ProtocolType.Tcp => ReceiveAsTcp(fileBatchesForMainPartCount),
			ProtocolType.Udp => ReceiveAsReliableUdp(fileBatchesForMainPartCount),
			_ => throw new InvalidOperationException($"Invalid protocol type {protocolTypeMessage.ProtocolType}")
		};
	}
	private IDictionary<int, FileBatch> ReceiveAsReliableUdp(int fileBatchesForMainPartCount)
	{
		return ReceiveFileBatches(fileBatchesForMainPartCount);
	}
	
	private IDictionary<int, FileBatch> ReceiveAsTcp(int fileBatchesForMainPartCount)
	{
		var server = new TcpListener(ServerIpAddress, _serverPort);
		using var client = server.AcceptTcpClient();
		using var networkStream = client.GetStream();
		
		var fileBatches = new Dictionary<int, FileBatch>();
		var buffer = new byte[BufferSize];
		
		for (var i = 0; i < fileBatchesForMainPartCount; i += BufferSize)
		{
			_ = networkStream.Read(buffer, 0, BufferSize);
			var fileBatch = GetMessage<FileBatch>(buffer);
			fileBatches.Add(fileBatch.Order, fileBatch);
		}
		
		server.Stop();
		return fileBatches;
	}

	private void SaveFile(FileBatchesMetadataMessage fileMetadata, IDictionary<int, FileBatch> fileBatches)
	{
		using var binaryWriter = new BinaryWriter(new FileStream(fileMetadata.FileName, FileMode.Create));
		binaryWriter.Write(fileBatches.OrderBy(b => b.Key).SelectMany(b => b.Value.Bytes).ToArray());
	}
	
	private IDictionary<int, FileBatch> ReceiveFileBatches(int fileBatchesCount)
	{
		var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
		var fileBatches = new Dictionary<int, FileBatch>();
		
		while (fileBatches.Count < fileBatchesCount)
		{
			var data = Server.Receive(ref remoteEndPoint);

			var fileBatch = GetMessage<FileBatch>(data);

			if (!fileBatches.ContainsKey(fileBatch.Order))
				fileBatches.Add(fileBatch.Order, fileBatch);

			_confirmStrategy.Confirm(fileBatch);
		}
		
		return fileBatches;
	}

	private TMessage GetMessage<TMessage>()
	{
		var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
		var data = Server.Receive(ref remoteEndPoint);
		return GetMessage<TMessage>(data);
	}

	private TMessage GetMessage<TMessage>(byte[] data)
	{
		var memory = new MemoryStream();
		memory.Write(data, 0, data.Length);
		memory.Position = 0;
		
		return (TMessage)BinaryFormatter.Deserialize(memory);
	}
	
	private void ChangeFileName(FileBatchesMetadataMessage message)
	{
		message.FileName = message.FileName.Replace(oldValue: "Client", newValue: "Server");
	}
}
