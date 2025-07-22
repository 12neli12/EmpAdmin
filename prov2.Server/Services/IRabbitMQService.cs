public interface IRabbitMQService
{
    void Publish(string queueName, string message);
}
