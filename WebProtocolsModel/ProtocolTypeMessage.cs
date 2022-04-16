using System;
namespace WebProtocolsModel
{
	[Serializable]
	public class ProtocolTypeMessage
	{
		public ProtocolType ProtocolType { get; set; }

		public static ProtocolTypeMessage TcpProtocolTypeMessage => new ProtocolTypeMessage { ProtocolType = ProtocolType.Tcp };
		public static ProtocolTypeMessage UdpProtocolTypeMessage => new ProtocolTypeMessage { ProtocolType = ProtocolType.Udp };
	}
	
	public enum ProtocolType
	{
		Tcp = 0,
		Udp = 1
	}
}
