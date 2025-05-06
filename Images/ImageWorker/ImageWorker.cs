using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using ImageMessageLib;
using System;
using System.Threading;

class ImageWorker
{
    static void Main()
    {
        //Configura la conexión a RabbitMQ
        var factory = new ConnectionFactory() { HostName = "192.168.1.65" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Declara el exchange de tipo 'topic' donde se publican/reciben los mensajes de imagen
        channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic);

        //Declara una cola persistente donde el worker recibirá las imágenes a procesar
        channel.QueueDeclare("ImageWorkQueue", true, false, false, null);

        //Instancia el procesador de imágenes (implementa IImageProcessor)
        IImageProcessor processor = new SimulatedProcessor(); // Se puede cambiar por uno real

        //Configura el consumidor que manejará los mensajes entrantes de la cola
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            //Extrae el contenido del mensaje
            var body = ea.Body.ToArray();
            var msg = Encoding.UTF8.GetString(body);
            var img = ImageMessage.Deserialize(msg); // Convierte el JSON en objeto

            Console.WriteLine($"[Worker] Procesando imagen: {img.Id}");

            //Procesa la imagen
            var processedPayload = processor.Process(img.Payload);

            //Crea un nuevo mensaje con la imagen ya procesada
            var resultMsg = new ImageMessage { Id = img.Id, Payload = processedPayload };
            var resultBody = Encoding.UTF8.GetBytes(resultMsg.Serialize());

            //Publica el mensaje procesado al exchange con una routing key distinta
            channel.BasicPublish("ImageExchange", "Image.Result", null, resultBody);
            Console.WriteLine($"[Worker] Resultado enviado: {resultMsg.Id}");
        };

        // Inicia el consumo de mensajes desde la cola
        channel.BasicConsume("ImageWorkQueue", true, consumer);
        Console.WriteLine("Trabajador esperando trabajo...");

        //Mantiene el programa corriendo
        Console.ReadLine();
    }

    //Clase de ejemplo que simula un procesamiento de imagen
    public class SimulatedProcessor : IImageProcessor
    {
        public string Process(string imagePayload)
        {
            Thread.Sleep(1000); //Simula un retardo de procesamiento (1 segundo)
            return $"Procesada_{imagePayload}"; // Devuelve un texto simulado como resultado
        }
    }
}
