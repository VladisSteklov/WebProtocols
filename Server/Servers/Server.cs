using WebProtocolsModel;

namespace Server.Servers
{
	public abstract class Server
	{
		protected readonly string serverAddress;

		protected readonly int serverPort;

		protected Server(string serverAddress, int serverPort)
		{
			this.serverAddress = serverAddress;
			this.serverPort = serverPort;
		}


		protected int bufferSize = ServerContext.BufferSize;
		public abstract void Process();
	}
}
