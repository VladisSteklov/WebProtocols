using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using WebProtocolsModel;

namespace Server.Servers.MultipleTransmissions;

internal class UseOneConnectionTcpServer : Server
{
	private readonly TcpListener _server;

	public UseOneConnectionTcpServer(string serverAddress, int serverPort) : base(serverAddress)
	{
		_server = new TcpListener(ServerIpAddress, serverPort);
	}

	public override void Process()
	{
		ShowStartServer();

		_server.Start();
		using var client = _server.AcceptTcpClient();
		using var networkStream = client.GetStream();

		ShowIncomingRequest();
		var stopwatch = new Stopwatch();
		stopwatch.Start();

		for (var i = 0; i < 3; i++)
		{
			var fileInfo = GetFileMetadata(networkStream);
			SaveFile(networkStream, fileInfo);
		}

		stopwatch.Stop();
		Console.WriteLine($"Файлы сохранены на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

		_server.Stop();
		Console.WriteLine("Выключение сервера");
	}

	private FileMetadata GetFileMetadata(Stream networkStream)
	{
		var binaryFormatter = new BinaryFormatter();
		var fileInfo = (FileMetadata)binaryFormatter.Deserialize(networkStream);
		fileInfo.FileName = fileInfo.FileName.Replace("Client", "Server");
		return fileInfo;
	}

	private void SaveFile(NetworkStream networkStream, FileMetadata fileMetadata)
	{
		using var file = new FileStream(fileMetadata.FileName, FileMode.Create);
		using var binaryWriter = new BinaryWriter(file);

		var bufCount = (int)Math.Ceiling(fileMetadata.FileSize * 1.0 / BufferSize);

		for (var i = 0; i < bufCount; i++)
		{
			var readBytes = i + 1 == bufCount ? (int)fileMetadata.FileSize - BufferSize * i : BufferSize;

			var buffer = new byte[readBytes];
			var count = networkStream.Read(buffer, 0, readBytes);
			binaryWriter.Write(buffer, 0, count);
		}
	}

	private static void ShowStartServer()
	{
		Console.WriteLine();
		Console.WriteLine("Старт сервера");
	}


	private static void ShowIncomingRequest()
	{
		Console.WriteLine();
		Console.WriteLine("Пришел запрос на сервер");
	}
}