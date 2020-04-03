using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RPCServer
{
    internal class RpcServer
    {
        private const string RpcQueue = "rpc_queue";

        private static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: RpcQueue, 
                durable: false,
                exclusive: false, 
                autoDelete: false, 
                arguments: null);

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(
                queue: RpcQueue,
                autoAck: false, 
                consumer: consumer);

            Console.WriteLine(" [x] Awaiting RPC requests");

            consumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    var n = int.Parse(message);
                    Console.WriteLine(" [.] fib({0})", message);
                    response = Fibonacci(n).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(
                        exchange: "", 
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps, 
                        body: responseBytes);

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false);
                }
            };

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static int Fibonacci(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return Fibonacci(n - 1) + Fibonacci(n - 2);
        }
    }
}
