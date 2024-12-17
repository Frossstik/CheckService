using MassTransit;
using CheckService.RabbitMQ;
using CheckService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();


builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CheckConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("user");
            h.Password("rabbitmq");
            h.Heartbeat(30); // ”величьте интервал heartbeat
        });

        cfg.ReceiveEndpoint("check-service", e =>
        {
            e.ConfigureConsumer<CheckConsumer>(context);

            e.Bind<CreateCheckMessage>(x =>
            {
                x.RoutingKey = "create-check-routing-key"; // ”кажите нужный routing key
            });
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapGrpcService<CheckServiceImplement>();

app.Run();
