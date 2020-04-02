using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace EmitLogDirect
{
    internal class EmitLogDirect
    {
        private const string ExchangeName = "direct_logs";

        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using var channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct);

                var severity = (args.Length > 0) ? args[0] : "info";

                var message = (args.Length > 1)
                    ? string.Join(" ", args.Skip(1).ToArray())
                    : "Hello World!";

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "direct_logs",
                    routingKey: severity,
                    basicProperties: null,
                    body: body);

                Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args, int index)
        {
            return ((args.Length > 0)
                ? string.Join(" ", args) + ": " + GetSeverity(index)
                : "Log type: " + GetSeverity(index));
        }

        private static string GetSeverity(int index)
        {
            return (index % 3) switch
            {
                0 => "info",
                1 => "warning",
                2 => "error",
                _ => "info"
            };
        }
    }
}
