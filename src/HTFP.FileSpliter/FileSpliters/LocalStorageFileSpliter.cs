using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HTFP.Shared.Storage;
using Microsoft.VisualBasic;

namespace HTFP.FileSpliter;

public sealed class LocalStorageFileSpliter : IFileSpliter
{
    private const int DefaultBufferSize = 1024 * 64;

    public async IAsyncEnumerable<Stream> SplitAsync(string path, int linesPerFile = 1000)
    {
        var newLine = (byte)'\n';

        var buffer = ArrayPool<byte>.Shared.Rent(DefaultBufferSize);
        
        try
        {
            using var fileStream = new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: DefaultBufferSize,
                        FileOptions.SequentialScan
                    );

            var ms = new MemoryStream();
            var lineCount = 0;

            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var start = 0;

                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] != newLine)
                        continue;

                    var lineLength = i - start + 1;

                    ms.Write(buffer, start, lineLength);
                    lineCount++;

                    if (lineCount >= linesPerFile)
                    {
                        ms.Position = 0;
                        yield return ms;

                        ms = new MemoryStream();

                        lineCount = 0;
                    }

                    start = i + 1;
                }

                if (start < bytesRead)
                    ms.Write(buffer, start, bytesRead - start);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
