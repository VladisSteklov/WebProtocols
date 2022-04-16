using System;
namespace WebProtocolsModel
{
	[Serializable]
	public class FileBatchesMetadataMessage
	{
		public string FileName { get; set; }
		public int FileBatchesForTestPartCount { get; set; }
		public int FileBatchesForMainPartCount { get; set; }
	}
}
