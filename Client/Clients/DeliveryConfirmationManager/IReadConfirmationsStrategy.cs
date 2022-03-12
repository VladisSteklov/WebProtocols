using System;
using WebProtocolsModel;

namespace Client.Clients.DeliveryConfirmationManager
{
	internal interface IReadConfirmationsStrategy : IDisposable
	{
		ConfirmMessage TryReadConfirmation();
	}
}
