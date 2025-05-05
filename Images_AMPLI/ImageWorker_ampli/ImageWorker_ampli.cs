using System;
using ImageMessageLib;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


namespace ImageWorker_ampli
{
    internal class ImageWorker_ampli
    {
        static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "192.168.1.65" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic);
            channel.QueueDeclare("ImageWorkQueue", true, false, false, null);

            IImageProcessor processor = new FaceDetectionProcessor();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);
                var img = ImageMessage.Deserialize(msg);

                Console.WriteLine($"[Worker] Procesando imagen: {img.Id}");
                var processedPayload = processor.Process(img.Payload);

                var resultMsg = new ImageMessage { Id = img.Id, Payload = processedPayload };
                var resultBody = Encoding.UTF8.GetBytes(resultMsg.Serialize());

                channel.BasicPublish("ImageExchange", "Image.Result", null, resultBody);
                Console.WriteLine($"[Worker] Resultado enviado: {resultMsg.Id}");
            };

            channel.BasicConsume("ImageWorkQueue", true, consumer);
            Console.WriteLine("Trabajador esperando trabajo...");
            Console.ReadLine();
        }
    }
}
