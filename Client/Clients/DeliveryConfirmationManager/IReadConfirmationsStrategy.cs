using System;
using WebProtocolsModel;

namespace Client.Clients.DeliveryConfirmationManager
{
	public interface IReadConfirmationsStrategy : IDisposable
	{
		ConfirmMessage TryReadConfirmation();
	}
}
