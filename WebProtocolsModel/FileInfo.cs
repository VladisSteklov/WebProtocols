using System;

namespace WebProtocolsModel
{
    [Serializable]
    public class FileInfo
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }
}
