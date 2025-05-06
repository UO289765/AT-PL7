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
        //Cola segura para múltiples hilos que almacena tuplas (imagen, routingKey)
        private static BlockingCollection<(ImageMessage img, string routingKey)> imageQueue = new();

        static void Main()
        {
            //Configura la conexión a RabbitMQ con la IP del servidor
            var factory = new ConnectionFactory() { HostName = "10.38.0.172" };
            using var connection = factory.CreateConnection(); //Abre la conexión
            using var channel = connection.CreateModel(); //Crea un canal de comunicación

            //Declara un exchange tipo 'topic' llamado ImageExchange (creado si no existe)
            channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic);

            //Declara una cola temporal (RabbitMQ genera el nombre automáticamente)
            var queueName = channel.QueueDeclare().QueueName;

            //Vincula la cola temporal al exchange para recibir mensajes con routingKey que empiecen por 'Image.'
            channel.QueueBind(queue: queueName, exchange: "ImageExchange", routingKey: "Image.*");

            //Crea un consumidor de eventos

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var img = ImageMessage.Deserialize(message);
                Console.WriteLine($"[Visualizer] Imagen {img.seqn} ({ea.RoutingKey}) recibida"); //Muestra por consola la imagen recibida

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
                byte[] imageBytes = Convert.FromBase64String(img.Payload); //convierte a bytes
                using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);

                if (mat.Empty())
                {
                    Console.WriteLine("Error al decodificar la imagen.");
                    return;
                }

                string windowName = routingKey switch
                {
                    "Image.Raw" => "Imagen sin procesar", //las raw son las originales, las result las procesadas
                    "Image.Result" => "Imagen procesada",
                    _ => "Imagen desconocida"
                };

                Cv2.ImShow(windowName, mat);
                Cv2.WaitKey(1);  //Refresca sin bloquear
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al mostrar la imagen: {ex.Message}");
            }
        }
    }
}
