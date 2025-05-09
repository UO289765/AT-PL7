﻿using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using ImageMessageLib;

namespace ImageProducer_ampliacion
{
    internal class ImageProducer_ampliacion
    {
        static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "10.38.0.172" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            const string exchangeName = "ImageExchange";
            const string routingKey = "Image.Raw";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic);

            IImageSource source = new WebcamImageSource(); //llama para capturar desde la webcam

            Console.WriteLine("Productor en tiempo real iniciado. Presiona Ctrl+C para detener.");

            while (true)
            {
                try
                {
                    var img = source.GetNextImage();  // Captura desde webcam
                    var body = Encoding.UTF8.GetBytes(img.Serialize());

                    channel.BasicPublish(
                        exchange: exchangeName,
                        routingKey: routingKey,
                        basicProperties: null,
                        body: body
                    );//Publica

                    Console.WriteLine($"[Producer] Imagen enviada: {img.seqn}");

                    Thread.Sleep(200);  // Espera 200 ms (~5 FPS)
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al capturar o enviar imagen: {ex.Message}");
                    Thread.Sleep(1000); // Espera más tiempo si falla
                }
            }
        }
    }
}
