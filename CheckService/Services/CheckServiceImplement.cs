using Grpc.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.IO;
using System.Text;
using Google.Protobuf;
using System.Threading.Tasks;

namespace CheckService.Services
{
    public class CheckServiceImplement : CheckService.CheckServiceBase
    {
        private readonly string _savePath = @"D:\Checks\";
        private readonly string _rabbitMqQueueName = "check_queue";

        public CheckServiceImplement()
        {
            Task.Run(() => StartRabbitMqConsumer());
        }

        public override Task<CreateCheckResponse> CreateCheck(CreateCheckRequest request, ServerCallContext context)
        {
            var filePath = GenerateCheck(request);
            return Task.FromResult(new CreateCheckResponse { FilePath = filePath });
        }

        private string GenerateCheck(CreateCheckRequest request)
        {
            string fileName = $"{request.OrderId}_check.txt";
            string filePath = Path.Combine(_savePath, fileName);

            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Order ID: {request.OrderId}");
                double TotalPrice = 0;
                foreach (var item in request.Items)
                {
                    writer.WriteLine($"{item.Name}: {item.Price}");
                    TotalPrice += item.Price;
                }
                writer.WriteLine($"Total price: {TotalPrice}");
            }

            return filePath;
        }

        private async Task StartRabbitMqConsumer()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",
                Password = "rabbitmq"
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(_rabbitMqQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Received raw message: {message}");

                    var request = CreateCheckRequest.Parser.ParseJson(message);

                    if (request != null)
                    {
                        var filePath = GenerateCheck(request);
                        Console.WriteLine($"Check generated for Order ID: {request.OrderId} at {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }

                await Task.Yield();
            };

            await channel.BasicConsumeAsync(queue: _rabbitMqQueueName, autoAck: true, consumer: consumer);
        }
    }
}
