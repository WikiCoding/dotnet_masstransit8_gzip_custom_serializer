using MassTransit;

namespace MT8_Demo;

public class MsgConsumer(ILogger<MsgConsumer> logger) : IConsumer<Msg>
{
    public Task Consume(ConsumeContext<Msg> context)
    {
        logger.LogInformation("Received: {}", context.Message.Id);
        return Task.CompletedTask;
    }
}
