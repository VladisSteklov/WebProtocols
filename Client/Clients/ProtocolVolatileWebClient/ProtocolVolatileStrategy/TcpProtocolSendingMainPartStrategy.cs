using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using WebProtocolsModel;

namespace Client.Clients.ProtocolVolatileWebClient.ProtocolVolatileStrategy;

public class TcpProtocolSendingMainPartStrategy : IProtocolSendingMainPartStrategy
{
    private readonly System.Net.Sockets.TcpClient _tcpClient;
    private readonly IPEndPoint _serverIpEndPoint;
    
    public TcpProtocolSendingMainPartStrategy(IPEndPoint serverIpEndPoint)
    {
        _tcpClient = new System.Net.Sockets.TcpClient();
        _serverIpEndPoint = serverIpEndPoint;
    }

    public ProtocolTypeMessage ProtocolTypeMessage => ProtocolTypeMessage.TcpProtocolTypeMessage;
    
    public void SendFileForMainPart(IDictionary<int, FileBatch> fileBatches)
    {
        _tcpClient.Connect(_serverIpEndPoint);

        using var networkStream = _tcpClient.GetStream();
        SendFile(networkStream, fileBatches);
        
        _tcpClient.Close();
    }
    
    private static void SendFile(Stream networkStream, IDictionary<int, FileBatch> fileBatches)
    {
        foreach (var sendingBytes in fileBatches.Values.Select(batch => batch.ToByteArray()))
        {
            networkStream.Write(sendingBytes, 0, sendingBytes.Length);
        }
    }
    
    public void Dispose()
    {
        _tcpClient?.Close();
    }
}