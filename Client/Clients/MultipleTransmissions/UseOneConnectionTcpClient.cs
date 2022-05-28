using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using WebProtocolsModel;

namespace Client.Clients.MultipleTransmissions;

internal class UseOneConnectionTcpClient : Client, IMultipleTransmissionClient
{
	private readonly System.Net.Sockets.TcpClient _tcpClient = new();

	public UseOneConnectionTcpClient(string serverIpAddress, int serverPort) : base(serverIpAddress, serverPort)
	{
	}

	public void SendFiles(params string[] fileNames)
	{
		_tcpClient.Connect(ServerIpEndPoint);

		using var networkStream = _tcpClient.GetStream();
		foreach (var fileName in fileNames)
		{
			SendFile(fileName, networkStream);
		}
	}

	private void SendFile(string fileName, System.Net.Sockets.NetworkStream networkStream)
	{
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException(fileName);
		}

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

		var binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(networkStream, fileInfo);
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
		_tcpClient?.Close();
	}
}