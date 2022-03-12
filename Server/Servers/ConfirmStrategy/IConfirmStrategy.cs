using System;

using WebProtocolsModel;

namespace Server.Servers.ConfirmStrategy
{
	internal interface IConfirmStrategy : IDisposable
	{
		void Confirm(FileBatch fileBatch);

		void StopConfirming();
	}
}
