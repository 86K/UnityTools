using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tools
{
    /// <summary>
    /// 以文件流形式作为HttpContent
    /// 需要进度值使用本Content，否则使用StreamContent
    /// </summary>
    public class StreamProgressContent : HttpContent
    {
        private const int defaultBufferSize = 4096;

        // 如果FileStream不适合作为流的父类或者本身，使用Stream
        private readonly FileStream content;
        private readonly int bufferSize;
        // 进度回调  已上传字节数 - 总字节数
        private readonly Action<long, long> progressCallback;

        public StreamProgressContent(FileStream content, Action<long, long> progressCallback)
            : this(content, defaultBufferSize, progressCallback)
        {
        }

        public StreamProgressContent(FileStream content, int bufferSize, Action<long, long> progressCallback)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this.content = content ?? throw new ArgumentNullException(nameof(content));
            this.bufferSize = bufferSize;
            this.progressCallback = progressCallback ?? throw new ArgumentNullException(nameof(progressCallback));

            Headers.ContentLength = content.Length;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[bufferSize];
            var uploadedBytes = 0L;

            using (content)
            {
                while (true)
                {
                    int bytesRead = await content.ReadAsync(buffer, 0, bufferSize);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    await stream.WriteAsync(buffer, 0, bytesRead);

                    uploadedBytes += bytesRead;
                    progressCallback?.Invoke(uploadedBytes, content.Length);
                }
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = content.Length;
            return true;
        }
    }
}
