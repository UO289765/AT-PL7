using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string BINDING_KEY = "alerta.*";
            const string EXCHANGE = "Alertas";

            var factory = new ConnectionFactory() { HostName = "192.168.1.65" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declarar el exchange tipo topic
                channel.ExchangeDeclare(EXCHANGE, ExchangeType.Topic);

                // Crear una cola temporal (nombre generado aleatoriamente)
                var queueName = channel.QueueDeclare().QueueName;

                // Enlazar la cola con el exchange usando el binding key
                channel.QueueBind(queue: queueName, exchange: EXCHANGE, routingKey: BINDING_KEY);

                Console.WriteLine($"[*] Escuchando mensajes con patrón '{BINDING_KEY}'...");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"[x] Recibido '{ea.RoutingKey}': '{message}'");
                };

                // Activar la escucha en la cola temporal
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                Console.WriteLine("Presiona Enter para salir.");
                Console.ReadLine();
            }
        }
    }
}
