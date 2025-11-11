using System.Text;
using RabbitMQ.Client;

namespace RabbitPOC;

public class RabbitConnectioRabbitPoc

{
    [Fact]
    public async Task ShouldConnect()
    {
        // RabbitMQ.Client 7.x, .NET 8


        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest", // raw, no escaping needed
            VirtualHost = "/" // default vhost
        };

        await using var conn = await factory.CreateConnectionAsync();
        await using var ch = await conn.CreateChannelAsync();

        string exchange = "ExchangePoc";

        // declare, bind (idempotent)
        await ch.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true);
        QueueDeclareOk qok = await ch.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true,
            arguments: null);
        var queue = qok.QueueName;
        await ch.QueueBindAsync(queue, exchange, "");

        // pull once (null if nothing ready)
        BasicGetResult? result = await ch.BasicGetAsync(queue, autoAck: true);
        if (result is null)
        {
            Console.WriteLine("No message available right now.");
            return;
        }

        var body = result.Body.ToArray();
        Console.WriteLine($"Message='{Encoding.UTF8.GetString(body)}'");
    }
}
