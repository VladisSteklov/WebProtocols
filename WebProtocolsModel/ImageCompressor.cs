using System.IO;
using System.IO.Compression;

namespace WebProtocolsModel
{
	public static class ImageCompressor
    {
		public static MemoryStream CompressGZip(FileStream fileStream)
        {
            var memoryStream = new MemoryStream();

            using (var compressionStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                fileStream.CopyTo(compressionStream);
            }

            return memoryStream;
        }

        public static void SaveAndDecompressGZip(string fileName, MemoryStream memoryStream)
		{
            using (var targetStream = File.Create(fileName))
            {
                using (var decompressionStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(targetStream);
                }
            }
        }

        public static MemoryStream CompressDeflate(FileStream fileStream)
        {
            var memoryStream = new MemoryStream();

            var compressionStream = new DeflateStream(memoryStream, CompressionMode.Compress);
            fileStream.CopyTo(compressionStream);

            return memoryStream;
        }

        public static void SaveAndDecompressDeflate(string fileName, MemoryStream memoryStream)
        {
            memoryStream.Position = 0;

            using (var targetStream = File.Create(fileName))
			{
                using (var decompressionStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(targetStream);
                }
            }
        }
    }
}
