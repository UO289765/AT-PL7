using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ImageMessageLib;
using OpenCvSharp;

namespace ImageVisualizer_ampli
{
    // Implementamos la interfaz IImageViewer
    class ImageVisualizer_ampli : IImageViewer
    {
        private static BlockingCollection<(ImageMessage img, string routingKey)> imageQueue = new();

        static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "10.38.0.172" };
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
                Console.WriteLine($"[Visualizer] Imagen {img.seqn} ({ea.RoutingKey}) recibida");

                // Encola la imagen para que el hilo principal la procese
                imageQueue.Add((img, ea.RoutingKey));
            };

            channel.BasicConsume(queueName, true, consumer);
            Console.WriteLine("Visualizador activo...");

            // Bucle del hilo principal para mostrar imágenes
            while (true)
            {
                var (img, routingKey) = imageQueue.Take();  // Espera bloqueante
                // Usamos el método Display de la interfaz IImageViewer
                DisplayImage(img, routingKey);
            }
        }

        // Implementamos la interfaz IImageViewer
        public void Display(ImageMessage img)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(img.Payload);
                using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);

                if (mat.Empty())
                {
                    Console.WriteLine("Error al decodificar la imagen.");
                    return;
                }

                string windowName = img.Payload.Contains("raw") ? "Imagen sin procesar" : "Imagen procesada";
                Cv2.ImShow(windowName, mat);
                Cv2.WaitKey(1);  // Refresca sin bloquear
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al mostrar la imagen: {ex.Message}");
            }
        }

        // Método para mostrar la imagen según su routingKey
        static void DisplayImage(ImageMessage img, string routingKey)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(img.Payload);
                using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);

                if (mat.Empty())
                {
                    Console.WriteLine("Error al decodificar la imagen.");
                    return;
                }

                string windowName = routingKey switch
                {
                    "Image.Raw" => "Imagen sin procesar",
                    "Image.Result" => "Imagen procesada",
                    _ => "Imagen desconocida"
                };

                Cv2.ImShow(windowName, mat);
                Cv2.WaitKey(1);  // Refresca sin bloquear
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al mostrar la imagen: {ex.Message}");
            }
        }
    }
}
