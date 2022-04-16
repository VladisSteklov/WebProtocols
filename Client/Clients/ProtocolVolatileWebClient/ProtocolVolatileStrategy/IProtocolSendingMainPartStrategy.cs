using System;
using System.Collections.Generic;
using WebProtocolsModel;
namespace Client.Clients.ProtocolVolatileWebClient.ProtocolVolatileStrategy;

public interface IProtocolSendingMainPartStrategy : IDisposable
{
    ProtocolTypeMessage ProtocolTypeMessage { get; }
    void SendFileForMainPart(IDictionary<int, FileBatch> fileBatches);
}