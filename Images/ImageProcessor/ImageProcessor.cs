using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using ImageMessageLib;
using System;

class ImageProcessor
{
    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "192.168.1.65" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic); //Declara el exchange para mensajes de imagen
        channel.QueueDeclare("ImageWorkQueue", true, false, false, null); //Declara la cola persistente de trabajo

        var queueName = channel.QueueDeclare().QueueName; //Declara una cola temporal autogenerada
        channel.QueueBind(queueName, "ImageExchange", "Image.Raw"); //Enlaza esa cola temporal al exchange con clave "Image.Raw"

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray(); //Extrae el cuerpo del mensaje
            var msg = Encoding.UTF8.GetString(body); //Convierte el cuerpo a texto
            var img = ImageMessage.Deserialize(msg); //Deserializa a objeto ImageMessage

            Console.WriteLine($"[Processor] Imagen recibida: {img.Id}, enviando a cola de trabajo");

            channel.BasicPublish("", "ImageWorkQueue", null, body); //Reenvía el mensaje a la cola de trabajo
        };

        channel.BasicConsume(queueName, true, consumer); //Empieza a consumir desde la cola temporal
        Console.WriteLine("Procesador activo...");
        Console.ReadLine(); //Evita que el programa termine
    }
}

