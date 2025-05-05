using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using ImageMessageLib;
namespace ImageProcessor_ampli
{
    class ImageProcessor_ampli
    {
        static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "192.168.1.65" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic);
            channel.QueueDeclare("ImageWorkQueue", true, false, false, null);

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, "ImageExchange", "Image.Raw");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);
                var img = ImageMessage.Deserialize(msg);

                Console.WriteLine($"[Processor] Imagen recibida: {img.Id}, enviando a cola de trabajo");

                channel.BasicPublish("", "ImageWorkQueue", null, body);
            };

            channel.BasicConsume(queueName, true, consumer);
            Console.WriteLine("Procesador activo...");
            Console.ReadLine();
        }
    }
}