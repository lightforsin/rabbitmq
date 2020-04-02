using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace NewTask
{
    internal class NewTask
    {
        private const string QueueName = "task_queue";

        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: QueueName, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            for (var i = 0; i < 1000; i++)
            {
                var message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: QueueName,
                    basicProperties: properties,
                    body: body);

                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Default") + " " + DateTime.Now.ToString("G");
        }
    }
}
