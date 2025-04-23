using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using ImageMessageLib;
using System;
using System.Collections.Generic;

class ImageVisualizer
{
    private static int expectedId = 0;  // El siguiente ID esperado
    private static Dictionary<int, ImageMessage> imageBuffer = new Dictionary<int, ImageMessage>();  // Buffer para imágenes desordenadas

    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "192.168.1.65" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic);

        var queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(queue: queueName, exchange: "ImageExchange", routingKey: "Image.*");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var img = ImageMessage.Deserialize(message);

            Console.WriteLine($"[Visualizer] Imagen {img.Id} ({ea.RoutingKey}): {img.Payload}");

            // Verificamos si la imagen es la siguiente esperada
            if (img.Id == expectedId)
            {
                // Muestra la imagen y procesa las siguientes si están en el buffer
                DisplayImage(img);
                expectedId++;

                // Muestra las imágenes que están en el buffer y tienen el ID esperado
                CheckBuffer();
            }
            else
            {
                // Si la imagen no es la esperada, la ponemos en el buffer
                if (!imageBuffer.ContainsKey(img.Id))
                {
                    imageBuffer.Add(img.Id, img);
                }
            }
        };

        channel.BasicConsume(queueName, true, consumer);
        Console.WriteLine("Visualizador activo...");
        Console.ReadLine();
    }

    // Muestra la imagen
    static void DisplayImage(ImageMessage img)
    {
        Console.WriteLine($"[Visualizer] Mostrando imagen: {img.Id}, {img.Payload}");
    }

    // Revisa si hay imágenes en el buffer que ahora pueden mostrarse
    static void CheckBuffer()
    {
        while (imageBuffer.ContainsKey(expectedId))
        {
            var img = imageBuffer[expectedId];
            imageBuffer.Remove(expectedId);
            DisplayImage(img);
            expectedId++;
        }
    }
}

