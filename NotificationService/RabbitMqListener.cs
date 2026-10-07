using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService
{
    // BackgroundService runs continuously in the background
    public class RabbitMqListener : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            // Connect to RabbitMQ
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            string queueName = "HighRiskVendorQueue";
            await channel.QueueDeclareAsync(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Create the consumer that listens to the queue
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Simulate business logic (e.g., sending an email)
                Console.WriteLine($"\n[!] ALERT RECEIVED: High Risk Vendor Detected!");
                Console.WriteLine($"[!] Message Data: {message}");

                await Task.Delay(1000, stoppingToken); // Simulate email delay
                Console.WriteLine("[!] Email successfully sent to ESG Admins.\n");
            };

            // Start consuming
            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

            // Keep the background service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
