using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace Send
{
    internal class Send
    {
        private const string QueueName = "PbQueue";

        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using var channel = connection.CreateModel();
                channel.QueueDeclare(queue: QueueName, 
                    durable: false, 
                    exclusive: false, 
                    autoDelete: false, 
                    arguments: null);

                for (var i = 0; i < 10; i++)
                {
                    Thread.Sleep(1000);
                    var message = "Message at " + DateTime.Now.ToString("G");
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                        routingKey: QueueName, // should match the queue name
                        basicProperties: null,
                        body: body);

                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
