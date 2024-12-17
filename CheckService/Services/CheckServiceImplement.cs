using Grpc.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;


namespace CheckService.Services
{
    public class CheckServiceImplement : CheckService.CheckServiceBase
    {
        private readonly string _savePath = @"D:\Checks\";

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
    }
}
