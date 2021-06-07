namespace Server.Servers
{
	public abstract class Server
	{
		protected int bufferSize = 512;
		public void SetBufferSize(int bufferSize) => this.bufferSize = bufferSize;
		public abstract void Process();
	}
}
