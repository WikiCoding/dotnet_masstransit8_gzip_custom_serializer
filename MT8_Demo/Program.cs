using MassTransit;
using MT8_Demo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MsgConsumer>();

    x.UsingRabbitMq((context, config) =>
    {
        config.Host(new Uri("amqp://localhost:5672"), configure: h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        config.ConfigureEndpoints(context);

        config.AddDeserializer(new GzipSerializerFactory(new System.Net.Mime.ContentType("application/vnd.masstransit+json")));
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
