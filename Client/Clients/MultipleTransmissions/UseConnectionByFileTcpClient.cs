using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using WebProtocolsModel;

namespace Client.Clients.MultipleTransmissions;

internal class UseConnectionByFileTcpClient : Client, IMultipleTransmissionClient
{
	private readonly BinaryFormatter _binaryFormatter = new();

	public UseConnectionByFileTcpClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
	{
	}

	public void SendFiles(params string[] fileNames)
	{
		foreach (var fileName in fileNames)
		{
			SendFile(fileName);
		}
	}

	private void SendFile(string fileName)
	{
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException(fileName);
		}

		using var tcpClient = new System.Net.Sockets.TcpClient();
		tcpClient.Connect(ServerIpEndPoint);
		using var networkStream = tcpClient.GetStream();

		using var fileStream = new FileStream(fileName, FileMode.Open);
		SendFileMetadata(networkStream, fileStream);
		SendFile(networkStream, fileStream);
	}

	private void SendFileMetadata(System.Net.Sockets.NetworkStream networkStream, FileStream fileStream)
	{
		var fileInfo = new FileMetadata
		{
			FileName = fileStream.Name,
			FileSize = fileStream.Length
		};

		_binaryFormatter.Serialize(networkStream, fileInfo);
	}

	private void SendFile(System.Net.Sockets.NetworkStream networkStream, FileStream fileStream)
	{
		using var binaryReader = new BinaryReader(fileStream);
		var buffer = new byte[BufferSize];
		int count;

		while ((count = binaryReader.Read(buffer, 0, BufferSize)) > 0)
		{
			networkStream.Write(buffer, 0, count);
		}
	}

	public override void Dispose()
	{
	}
}