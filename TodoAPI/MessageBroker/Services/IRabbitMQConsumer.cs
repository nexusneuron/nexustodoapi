namespace TodoAPI.MessageBroker.Services
{
    public interface IRabbitMQConsumer
    {
        //Task ConsumeMessageAsync<T>(T message, string queueName);
        Task ConsumeMessageAsync(string queueName);
    }
}
