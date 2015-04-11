using System;
#if UNIVERSAL
using Windows.Web.Http;
using Windows.Storage;
using Windows.Storage.Streams;
#else
using System.Net.Http;
using System.IO;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace Nulands.Restless.Extensions
{

    public static class RestResponseExtensions
    {
        public static async Task DownloadFromResponse(
            this RestResponse<IVoid> response,
            Action<int> progress,
#if UNIVERSAL
            Action<IBuffer> writeAction,
#else
            Action<byte[]> writeAction,
#endif
            CancellationToken cancelToken)
        {
            await response.HttpResponse.DownloadFromResponse(progress, writeAction, cancelToken);
        }

        public static async Task DownloadFromResponse(
            this RestResponse<IVoid> response,
#if UNIVERSAL
            IOutputStream targetStream,
#else
            Stream targetStream,
#endif
            Action<int> progress,
            CancellationToken cancelToken)
        {
            await response.DownloadFromResponse(
                progress
#if UNIVERSAL
                , async buffer => await targetStream.WriteAsync(buffer)
#else
                , buffer => targetStream.WriteAsync(buffer, 0, buffer.Length)
#endif
                , cancelToken);
        }

        public static async Task DownloadFromResponse(
            this HttpResponseMessage response,
            Action<int> progress,
#if UNIVERSAL
            Action<IBuffer> writeAction,
#else
            Action<byte[]> writeAction,
#endif
            CancellationToken cancelToken)
        {
#if UNIVERSAL
            using (IInputStream input = await response.Content.ReadAsInputStreamAsync())
            {
                DataReader reader = new DataReader(input);

                ulong numberOfReads = response.Content.Headers.ContentLength.Value / 4096;
                ulong currentNumerOfReads = 0;
                while (reader.UnconsumedBufferLength > 0)
                {
                    IBuffer buff = reader.ReadBuffer(4096);
                    progress((int)((currentNumerOfReads / numberOfReads) * 100));
                    writeAction(buff);
                    currentNumerOfReads++;
                }
            }
#else
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                long numberOfReads = response.Content.Headers.ContentLength.Value / 4096;
                long currentNumerOfReads = 0;

                var buffer = new byte[4096];
                while ((await stream.ReadAsync(buffer, 0, 4096, cancelToken) > 0))
                {
                    progress((int)((currentNumerOfReads / numberOfReads) * 100));
                    writeAction(buffer);
                    currentNumerOfReads++;
                }
            }
#endif
        }

        public static async Task DownloadFromResponse(
            this HttpResponseMessage response,
            Action<int> progress,
#if UNIVERSAL
            Func<IBuffer, Windows.Foundation.IAsyncOperationWithProgress<int, int>> writeAction,
#else
            Func<byte[], Task> writeAction,
#endif
            CancellationToken cancelToken)
        {
#if UNIVERSAL
            using (IInputStream input = await response.Content.ReadAsInputStreamAsync())
            {
                DataReader reader = new DataReader(input);
                ulong numberOfReads = response.Content.Headers.ContentLength.Value / 4096;
                ulong currentNumerOfReads = 0;
                while (reader.UnconsumedBufferLength > 0)
                {
                    IBuffer buff = reader.ReadBuffer(4096);
                    progress((int)((currentNumerOfReads / numberOfReads) * 100));
                    await writeAction(buff);
                    currentNumerOfReads++;
                }
            }
#else
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                long numberOfReads = response.Content.Headers.ContentLength.Value / 4096;
                long currentNumerOfReads = 0;

                var buffer = new byte[4096];
                while ((await stream.ReadAsync(buffer, 0, 4096, cancelToken) > 0))
                {
                    progress((int)((currentNumerOfReads / numberOfReads) * 100));
                    await writeAction(buffer);
                    currentNumerOfReads++;
                }
            }
#endif
        }

        public static async Task DownloadFromResponse(
            this HttpResponseMessage response,
#if UNIVERSAL
            IOutputStream targetstream,
#else
            Stream targetStream,
#endif
            Action<int> progress,
            CancellationToken cancelToken)
        {
            await response.DownloadFromResponse(
                progress
#if UNIVERSAL
                , async buffer => await targetstream.WriteAsync(buffer)
#else
                , buffer => targetStream.WriteAsync(buffer, 0, buffer.Length)
#endif
                , cancelToken);
        }
    }
}
