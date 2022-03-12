using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Server.Servers.ConfirmStrategy;
using Server.Servers.ConfirmStrategy.ConfirmStrategyFactory;

using WebProtocolsModel;

namespace Server.Servers
{
	internal abstract class ReliableUdpServer : UdpServer
	{
		private readonly IConfirmStrategyFactory _confirmStrategyFactory;

		internal ReliableUdpServer(string serverAddress, int serverPort, IConfirmStrategyFactory confirmStrategyFactory)
			: base(serverAddress, serverPort)
		{
			_confirmStrategyFactory = confirmStrategyFactory;
		}

		public override void Process()
		{
			var fileMetadata = GetFileMetadata();
			using var confirmStrategy = _confirmStrategyFactory.CreateStrategy(ServerIpAddress);

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			SaveFile(fileMetadata, confirmStrategy);

			stopwatch.Stop();
			Console.WriteLine($"Файл сохранен на сервере за {stopwatch.ElapsedMilliseconds} милисекунд");

			Server.Close();
			Console.WriteLine("Выключение сервера");
		}

		protected abstract void SaveFile(FileMetadata fileMetadata, IConfirmStrategy confirmStrategy);

		protected static FileBatch GetFileBatch(byte[] data)
		{
			var memory = new MemoryStream();
			memory.Write(data, 0, data.Length);
			memory.Position = 0;

			return new BinaryFormatter().Deserialize(memory) as FileBatch;
		}
	}
}
