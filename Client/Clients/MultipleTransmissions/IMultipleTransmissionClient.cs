namespace Client.Clients.MultipleTransmissions;

internal interface IMultipleTransmissionClient
{
	void SendFiles(params string[] fileNames);
}