using MassTransit;

namespace MT8_Demo;

public class MsgProducer(IPublishEndpoint publishEndpoint)
{

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await publishEndpoint.Publish(new Msg(), cancellationToken);
    }
}
