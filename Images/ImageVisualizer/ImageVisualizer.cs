using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using ImageMessageLib;
using System;
using System.Collections.Generic;

class ImageVisualizer : IImageViewer
{
    private static int expectedId = 0; // El ID de la próxima imagen esperada
    private static Dictionary<int, ImageMessage> imageBuffer = new Dictionary<int, ImageMessage>(); // Buffer para mantener imágenes en espera

    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "192.168.1.65" }; //IP del host
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        //Declarar el intercambio topic
        channel.ExchangeDeclare("ImageExchange", ExchangeType.Topic);

        //Crear una cola temporal para recibir todas las imágenes (Image.*)
        var queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(queue: queueName, exchange: "ImageExchange", routingKey: "Image.*");

        var visualizer = new ImageVisualizer();

        //Configurar el consumidor de RabbitMQ
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var img = ImageMessage.Deserialize(message);

                //Imprimir la imagen en consola con los colores correctos a través del método Display
                visualizer.Display(img);

                // Si la imagen es la esperada, la mostramos
                if (img.Id == expectedId)
                {
                    expectedId++;
                    visualizer.CheckBuffer(); //Verificamos si hay imágenes en el buffer para mostrar
                }
                else
                {
                    //Si no es la imagen esperada, la guardamos en el buffer
                    if (!imageBuffer.ContainsKey(img.Id))
                    {
                        imageBuffer.Add(img.Id, img);
                    }
                }

                //Confirmar recepción de mensaje
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        //Comenzamos a consumir mensajes
        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        Console.WriteLine("Visualizador activo...");
        Console.ReadLine();
    }

    //Método para mostrar una imagen
    public void Display(ImageMessage img)
    {
        //Si la imagen tiene el prefijo "Procesada_", la tratamos como imagen procesada
        if (img.Payload.StartsWith("Procesada_"))
        {
            Console.ForegroundColor = ConsoleColor.Green; // Color verde para imagen procesada
            Console.WriteLine($"[IMAGEN PROCESADA] ID: {img.Id}, Datos: {img.Payload}");
        }
        else //Si no, la imagen es original
        {
            Console.ForegroundColor = ConsoleColor.Cyan; //Color cyan para imagen original
            Console.WriteLine($"[IMAGEN ORIGINAL] ID: {img.Id}, Datos: {img.Payload}");
        }
        Console.ResetColor(); //Resetear el color de la consola
    }

    //Método para comprobar el buffer y mostrar imágenes en orden
    private void CheckBuffer()
    {
        while (imageBuffer.ContainsKey(expectedId))
        {
            var img = imageBuffer[expectedId];
            imageBuffer.Remove(expectedId); //Eliminar la imagen del buffer
            Display(img); //Mostrarla usando el método Display para aplicar colores
            expectedId++; //Actualizar el ID esperado
        }
    }
}
