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
            var factory = new ConnectionFactory() { HostName = "10.38.0.172" }; //Crea la conexión a RabbitMQ usando la IP especificada
            using var connection = factory.CreateConnection(); //Establece la conexión
            using var channel = connection.CreateModel(); //Crea un canal de comunicación con RabbitMQ

            channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic); //Declara un exchange de tipo 'topic' llamado ImageExchange
            channel.QueueDeclare("ImageWorkQueue", true, false, false, null); //Declara una cola persistente llamada ImageWorkQueue

            var queueName = channel.QueueDeclare().QueueName; //Declara una cola temporal y guarda su nombre
            channel.QueueBind(queueName, "ImageExchange", "Image.Raw"); //Vincula la cola temporal al exchange usando el routing key 'Image.Raw'

            var consumer = new EventingBasicConsumer(channel); //Crea un consumidor para recibir mensajes
            consumer.Received += (model, ea) => //Maneja el evento cuando se recibe un mensaje
            {
                var body = ea.Body.ToArray(); //Obtiene el cuerpo del mensaje en forma de byte[]
                var msg = Encoding.UTF8.GetString(body); //Convierte los bytes a string
                var img = ImageMessage.Deserialize(msg); //Deserializa el string a un objeto ImageMessage

                Console.WriteLine($"[Processor] Imagen recibida: {img.seqn}, enviando a cola de trabajo"); //Muestra información de la imagen recibida

                channel.BasicPublish("", "ImageWorkQueue", null, body); //Publica el mensaje en la cola de trabajo
            };

            channel.BasicConsume(queueName, true, consumer); //Empieza a consumir mensajes desde la cola temporal
            Console.WriteLine("Procesador activo..."); //Indica que el procesador está en funcionamiento
            Console.ReadLine(); //Evita que el programa finalice inmediatamente
        }
    }
}
