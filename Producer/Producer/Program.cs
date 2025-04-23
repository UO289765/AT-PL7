using System;
using System.Text;
using RabbitMQ.Client;

namespace Producer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "192.168.1.65" };
            const string EXCHANGE = "Alertas";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declarar el exchange tipo topic
                channel.ExchangeDeclare(EXCHANGE, ExchangeType.Topic);

                while (true)
                {
                    Console.Write("Tipo de alerta (ej. alerta.info, alerta.crítica): ");
                    string routingKey = Console.ReadLine();

                    Console.Write("Mensaje: ");
                    string message = Console.ReadLine();

                    var body = Encoding.UTF8.GetBytes(message);

                    // Publicar el mensaje al exchange con la clave
                    channel.BasicPublish(exchange: EXCHANGE, routingKey: routingKey, basicProperties: null, body: body);
                    Console.WriteLine($"[x] Enviado '{routingKey}':'{message}'");
                }
            }
        }
    }
}
