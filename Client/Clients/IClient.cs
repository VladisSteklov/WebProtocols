using System;

namespace Client.Clients;

internal interface IClient : IDisposable
{
	void SendFile(string fileName);

}