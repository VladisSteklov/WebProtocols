using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using WebProtocolsModel;

namespace Server.Servers.MultipleTransmissions;

internal class UseConnectionByFileTcpServer : Server
{
	private readonly TcpListener _server;

	private readonly BinaryFormatter _binaryFormatter = new();

	public UseConnectionByFileTcpServer(string serverAddress, int serverPort) : base(serverAddress)
	{
		_server = new TcpListener(ServerIpAddress, serverPort);
	}

	public override void Process()
	{
		ShowStartServer();

		_server.Start();
		var stopwatch = new Stopwatch();

		for (var i = 0; i < 3; i++)
		{
			using var client = _server.AcceptTcpClient();
			using var networkStream = client.GetStream();

			if (i == 0)
				stopwatch.Start();

			ShowIncomingRequest();

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
		var fileInfo = (FileMetadata)_binaryFormatter.Deserialize(networkStream);
		fileInfo.FileName = fileInfo.FileName.Replace("Client", "Server");
		return fileInfo;
	}

	private void SaveFile(NetworkStream networkStream, FileMetadata fileMetadata)
	{
		using var file = new FileStream(fileMetadata.FileName, FileMode.Create);
		using var binaryWriter = new BinaryWriter(file);

		for (var i = 0; i < fileMetadata.FileSize; i += BufferSize)
		{
			var buffer = new byte[BufferSize];
			var count = networkStream.Read(buffer, 0, BufferSize);
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