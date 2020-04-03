using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RpcClient
{
    public class RpcClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly BlockingCollection<string> _respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties _props;

        public RpcClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);

            _props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            _props.CorrelationId = correlationId;
            _props.ReplyTo = _replyQueueName;

            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    _respQueue.Add(response);
                }
            };
        }

        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: _props,
                body: messageBytes);

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true);

            return _respQueue.Take();
        }

        public void Close()
        {
            _connection.Close();
        }
    }

    public class Rpc
    {
        public static void Main(string[] args)
        {
            var rpcClient = new RpcClient();
            var value = args.Length > 0 ? args[0] : "20";

            for (var i = 1; i < 30; i++)
            {
                Thread.Sleep(200);
                Console.WriteLine(" [x] Requesting Fibonacci for " + i);
                var response = rpcClient.Call(i.ToString());

                Console.WriteLine(" [.] Got '{0}'", response);
            }

            rpcClient.Close();
        }
    }
}
