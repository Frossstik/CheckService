using MassTransit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CheckService.RabbitMQ
{
    public class CheckConsumer : IConsumer<CreateCheckMessage>
    {
        private readonly string _savePath = @"D:\Checks\";

        public async Task Consume(ConsumeContext<CreateCheckMessage> context)
        {
            var message = context.Message;
            try
            {
                // Генерируем чек и сохраняем путь
                var filePath = GenerateCheck(message);

                // Отправляем путь к файлу обратно
                await context.RespondAsync(new CreateCheckResponse { FilePath = filePath });

                // Логируем полученные данные
                Console.WriteLine($"Received OrderId: {message.OrderId}, ItemsCount: {message.Items.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private string GenerateCheck(CreateCheckMessage message)
        {
            string fileName = $"{message.OrderId}_check.txt";
            string filePath = Path.Combine(_savePath, fileName);

            // Генерация файла чека
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Order ID: {message.OrderId}");
                double totalPrice = 0;

                foreach (var item in message.Items)
                {
                    writer.WriteLine($"{item.Name}: {item.Price}");
                    totalPrice += item.Price;
                }

                writer.WriteLine($"Total price: {totalPrice}");
            }

            return filePath;
        }
    }
}
