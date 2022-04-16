using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Client.Clients.DeliveryConfirmationManager;

using WebProtocolsModel;

namespace Client.Clients.ProtocolVolatileWebClient.ProtocolVolatileStrategy;

public class UdpProtocolSendingMainPartStrategy : IProtocolSendingMainPartStrategy
{
    private readonly System.Net.Sockets.UdpClient _sendingUdpClient;
    private readonly IPEndPoint _serverIpEndPoint;
    private readonly DeliveryConfirmationHost _deliveryConfirmationHost;

    public UdpProtocolSendingMainPartStrategy(System.Net.Sockets.UdpClient sendingUdpClient,
        IPEndPoint serverIpEndPoint,
        DeliveryConfirmationHost deliveryConfirmationHost)
    {
        _sendingUdpClient = sendingUdpClient;
        _serverIpEndPoint = serverIpEndPoint;
        _deliveryConfirmationHost = deliveryConfirmationHost;
    }

    public ProtocolTypeMessage ProtocolTypeMessage => ProtocolTypeMessage.UdpProtocolTypeMessage;
    
    public void SendFileForMainPart(IDictionary<int, FileBatch> fileBatches)
    {
	    Console.WriteLine("Отправка главной части по UDP");

        var hostTask = _deliveryConfirmationHost.RunHostAsync();
        
        foreach (var sendingBytes in fileBatches.Values.Select(batch => batch.ToByteArray()))
        {
            _sendingUdpClient.Send(sendingBytes.ToArray(), sendingBytes.Length, _serverIpEndPoint);
        }
        
        hostTask.Wait();
    }
    
    public void Dispose()
    {
        _deliveryConfirmationHost?.Dispose();
        _sendingUdpClient?.Close();
    }
}