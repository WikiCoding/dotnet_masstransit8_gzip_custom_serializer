using MassTransit;
using MassTransit.Initializers;
using MassTransit.Initializers.TypeConverters;
using MassTransit.Serialization;
using System.IO.Compression;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace MT8_Demo;

public class GzipDeserializer : IMessageDeserializer, IObjectDeserializer
{
    private readonly ContentType contentType;

    public static JsonSerializerOptions Options => JsonSerializerOptions.Default;

    public GzipDeserializer(ContentType contentType)
    {
        this.contentType = contentType;
    }

    public ContentType ContentType => contentType;

    public ConsumeContext Deserialize(ReceiveContext receiveContext)
    {
        return new BodyConsumeContext(receiveContext,
            Deserialize(receiveContext.Body,
            receiveContext.TransportHeaders,
            receiveContext.InputAddress));
    }

    public SerializerContext Deserialize(MessageBody body, Headers headers, Uri? destinationAddress = null)
    {
        try
        {
            var decompressedJson = GzipDecompress(Convert.FromBase64String(Encoding.UTF8.GetString(body.GetBytes())));

            var jsonBody = JsonSerializer.Deserialize<Msg>(decompressedJson, Options);

            JsonElement? bodyElement = JsonSerializer.Deserialize<JsonElement>(decompressedJson, Options);

            if (bodyElement == null)
                throw new SerializationException("The body element is null, unable to deserialize the message envelope");

            var messageTypes = headers.GetMessageTypes();

            var messageContext = new RawMessageContext(headers, destinationAddress, RawSerializerOptions.Default);

            var serializerContext = new SystemTextJsonRawSerializerContext(SystemTextJsonMessageSerializer.Instance,
                    SystemTextJsonMessageSerializer.Options, ContentType, messageContext, messageTypes, RawSerializerOptions.Default, bodyElement.Value);

            return serializerContext;
        }
        catch (SerializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SerializationException("An error occured while deserializing the message envelope", ex);
        }
    }

    public MessageBody GetMessageBody(string text)
    {
        return new StringMessageBody(text);
    }

    public void Probe(ProbeContext context)
    {

    }

    public T? DeserializeObject<T>(object? value, T? defaultValue = null) where T : class
    {
        switch (value)
        {
            case null:
                return defaultValue;
            case T returnValue:
                return returnValue;
            case string text when string.IsNullOrWhiteSpace(text):
                return defaultValue;
            case string text when TypeConverterCache.TryGetTypeConverter(out ITypeConverter<T, string>? typeConverter)
                    && typeConverter.TryConvert(text, out var result):
                return result;
            case string text:
                return JsonSerializer.Deserialize<JsonElement>(text, Options).GetObject<T>(Options);
            case JsonElement jsonElement:
                return jsonElement.GetObject<T>(Options);
        }

        var element = JsonSerializer.SerializeToElement(value, Options);

        return element.ValueKind == JsonValueKind.Null
            ? defaultValue
            : element.GetObject<T>(Options);
    }

    public T? DeserializeObject<T>(object? value, T? defaultValue = null) where T : struct
    {
        switch (value)
        {
            case null:
                return defaultValue;
            case T returnValue:
                return returnValue;
            case string text when string.IsNullOrWhiteSpace(text):
                return defaultValue;
            case string text when TypeConverterCache.TryGetTypeConverter(out ITypeConverter<T, string>? typeConverter)
                    && typeConverter.TryConvert(text, out var result):
                return result;
            case string text:
                return JsonSerializer.Deserialize<T>(text, Options);
            case JsonElement jsonElement:
                return jsonElement.Deserialize<T>(Options);
        }

        var element = JsonSerializer.SerializeToElement(value, Options);

        return element.ValueKind == JsonValueKind.Null
            ? defaultValue
            : element.Deserialize<T>(Options);
    }

    public MessageBody SerializeObject(object? value)
    {
        if (value == null)
            return new EmptyMessageBody();

        return new SystemTextJsonObjectMessageBody(value, Options);
    }

    private string GzipDecompress(byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            var bts = new byte[4096];
            int cnt;

            while ((cnt = gs.Read(bts, 0, bts.Length)) != 0)
            {
                mso.Write(bts, 0, cnt);
            }
        }

        return Encoding.UTF8.GetString(mso.ToArray());
    }
}
