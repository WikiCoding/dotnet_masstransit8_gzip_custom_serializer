using MassTransit;
using MassTransit.Serialization;
using System.IO.Compression;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace MT8_Demo;

public class GzipSerializer : IMessageSerializer
{
    private readonly ContentType contentType;

    public GzipSerializer(ContentType contentType)
    {
        this.contentType = contentType;
    }

    public ContentType ContentType => contentType;

    public MessageBody GetMessageBody<T>(SendContext<T> context) where T : class
    {
        var payload = context.Message;

        if (payload == null) throw new ArgumentNullException(nameof(payload));

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        using var stream = new MemoryStream();
        using (var gzip = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true))
        {
            gzip.Write(bytes);
        }

        stream.Position = 0;
        var compressed = stream.ToArray();

        return new SystemTextJsonRawMessageBody<T>(context, JsonSerializerOptions.Default, compressed);
    }
}
