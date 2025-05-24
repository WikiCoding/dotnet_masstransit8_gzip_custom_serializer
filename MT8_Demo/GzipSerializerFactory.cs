using MassTransit;
using System.Net.Mime;

namespace MT8_Demo;

public class GzipSerializerFactory : ISerializerFactory
{
    private readonly ContentType contentType;

    public GzipSerializerFactory(ContentType contentType)
    {
        this.contentType = contentType;
    }

    public ContentType ContentType => contentType;

    public IMessageDeserializer CreateDeserializer()
    {
        return new GzipDeserializer(contentType);
    }

    public IMessageSerializer CreateSerializer()
    {
        return null;
    }
}
