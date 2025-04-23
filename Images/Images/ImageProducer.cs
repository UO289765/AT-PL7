using RabbitMQ.Client;
using System;
using System.Text;
using ImageMessageLib;

class ImageProducer
{
    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "192.168.1.65" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        const string exchangeName = "ImageExchange";
        const string routingKey = "Image.Raw";

        channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);

        IImageSource source = new SimulatedImageSource();

        for (int i = 0; i < 10; i++)
        {
            var img = source.GetNextImage();
            var body = Encoding.UTF8.GetBytes(img.Serialize());

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($"[Producer] Enviada imagen: {img.Id}");
        }

        Console.WriteLine("Presiona Enter para salir...");
        Console.ReadLine();
    }
}

// Clase que implementa la fuente de imágenes
class SimulatedImageSource : IImageSource
{
    private int id = 1;

    public ImageMessage GetNextImage()
    {
        return new ImageMessage
        {
            Id = id++,
            Payload = $"ImageData_{Guid.NewGuid()}"
        };
    }
}
