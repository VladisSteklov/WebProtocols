using System;

namespace WebProtocolsModel
{
    [Serializable]
    public class FileMetadata
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }
}
