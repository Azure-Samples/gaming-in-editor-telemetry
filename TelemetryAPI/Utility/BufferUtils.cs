using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryAPI.Utility
{
    /// <summary>
    /// Utility functions using recycled memory buffers
    /// </summary>
    public class BufferUtils
    {
        static RecyclableMemoryStreamManager _msManager = new RecyclableMemoryStreamManager();

        /// <summary>
        /// Get a recycled memory stream
        /// </summary>
        /// <returns></returns>
        public static MemoryStream GetStream()
        {
            return _msManager.GetStream();
        }

        /// <summary>
        /// Compress the memory stream using GZIP
        /// </summary>
        /// <param name="content"></param>
        /// <param name="compressionLevel"></param>
        /// <returns></returns>
        public static byte[] GzipCompressToArray(MemoryStream content, CompressionLevel compressionLevel)
        {
            using (var ms = _msManager.GetStream())
            {
                using (var gz = new GZipStream(ms, compressionLevel, true))
                {
                    content.WriteTo(gz);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Decompress a GZIP'd stream to bytes
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task<byte[]> GzipDecompressToArray(byte[] content)
        {
            using (var ms = new MemoryStream(content))
            using (var output = _msManager.GetStream())
            {
                using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                {
                    await gz.CopyToAsync(output);
                    return output.ToArray();
                }
            }
        }

        /// <summary>
        /// Decompress a GZIP'd stream to a string
        /// </summary>
        /// <param name="content"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> GzipDecompressToString(Stream content, Encoding encoding)
        {
            using (var gz = new GZipStream(content, CompressionMode.Decompress))
            using (var sr = new StreamReader(gz, encoding))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
}
