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
            // Crea una conexión con RabbitMQ en la IP indicada
            var factory = new ConnectionFactory() { HostName = "10.38.0.172" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declara el exchange tipo topic y la cola persistente que se usará para procesar imágenes
            channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic);
            channel.QueueDeclare("ImageWorkQueue", true, false, false, null);

            // Instancia un procesador que detecta caras en las imágenes (implementa IImageProcessor)
            IImageProcessor processor = new FaceDetectionProcessor();

            // Crea un consumidor que escucha mensajes en la cola de trabajo
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                // Deserializa el mensaje recibido desde RabbitMQ
                var body = ea.Body.ToArray();
                var msg = Encoding.UTF8.GetString(body);
                var img = ImageMessage.Deserialize(msg);

                Console.WriteLine($"[Worker] Procesando imagen: {img.seqn}");

                // Procesa la imagen usando el algoritmo configurado
                var processedPayload = processor.Process(img.Payload);

                // Crea un nuevo mensaje con el resultado procesado
                var resultMsg = new ImageMessage { seqn = img.seqn, Payload = processedPayload };
                var resultBody = Encoding.UTF8.GetBytes(resultMsg.Serialize());

                // Envía la imagen procesada de vuelta al exchange usando la clave "Image.Result"
                channel.BasicPublish("ImageExchange", "Image.Result", null, resultBody);
                Console.WriteLine($"[Worker] Resultado enviado: {resultMsg.seqn}");
            };

            // Empieza a consumir mensajes de la cola de trabajo
            channel.BasicConsume("ImageWorkQueue", true, consumer);
            Console.WriteLine("Trabajador esperando trabajo...");
            Console.ReadLine(); // Mantiene la aplicación corriendo
        }
    }
}
