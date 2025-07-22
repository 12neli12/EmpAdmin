using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class RabbitMqListener : BackgroundService
{
    private readonly ILogger<RabbitMqListener> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqListener(ILogger<RabbitMqListener> logger)
    {
        _logger = logger;
        _factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "myqueue",
                              durable: true,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message: {Message}", message);
            // TODO: handle the message
        };

        _channel.BasicConsume(queue: "myqueue",
                              autoAck: true,
                              consumer: consumer);

        stoppingToken.Register(() =>
        {
            _logger.LogInformation("Cancellation requested. Closing RabbitMQ connection.");
            Dispose();
        });

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
