using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Server.Servers.ConfirmStrategy;
using Server.Servers.ConfirmStrategy.ConfirmStrategyFactory;
using WebProtocolsModel;

namespace Server.Servers
{
	internal abstract class ReliableFastUdpServer : ReliableUdpServer
	{
		protected ReliableFastUdpServer(
			string serverAddress, int serverPort, IConfirmStrategyFactory confirmStrategyFactory)
			: base(serverAddress, serverPort, confirmStrategyFactory)
		{
		}

		protected override void SaveFile(FileMetadata fileMetadata, IConfirmStrategy confirmStrategy)
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
			var fileBatches = new Dictionary<int, FileBatch>();

			using (var binaryWriter = new BinaryWriter(new FileStream(fileMetadata.FileName, FileMode.Create)))
			{
				while (fileBatches.Count < Math.Ceiling((double)fileMetadata.FileSize / BufferSize))
				{
					var data = Server.Receive(ref remoteEndPoint);

					var fileBatch = GetFileBatch(data);

					if (!fileBatches.ContainsKey(fileBatch.Order))
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
