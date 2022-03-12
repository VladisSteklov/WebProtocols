using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Server.Servers.ConfirmStrategy;
using Server.Servers.ConfirmStrategy.ConfirmStrategyFactory;
using WebProtocolsModel;

namespace Server.Servers
{
	internal class ReliableSlowUdpServerTcpConfirm : ReliableUdpServer
	{
		internal ReliableSlowUdpServerTcpConfirm(string serverAddress, int serverPort)
			: base(serverAddress, serverPort, new TcpConfirmStrategyFactory())
		{
		}

		protected override void SaveFile(FileMetadata fileMetadata, IConfirmStrategy confirmStrategy)
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
			var fileBatches = new Dictionary<int, FileBatch>();

			using (var binaryWriter = new BinaryWriter(new FileStream(fileMetadata.FileName, FileMode.Create)))
			{
				for (var i = 0; i < fileMetadata.FileSize; i += BufferSize)
				{
					var data = Server.Receive(ref remoteEndPoint);

					var fileBatch = GetFileBatch(data);
					fileBatches.Add(fileBatch.Order, fileBatch);

					confirmStrategy.Confirm(fileBatch);
				}

				binaryWriter.Write(
					fileBatches
						.OrderBy(b => b.Key)
						.SelectMany(b => b.Value.Bytes)
						.ToArray());
			}
		}
	}
}
