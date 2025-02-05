﻿namespace TodoAPI.MessageBroker.Services
{
    //public interface IRabbitMQPublisher<T>
    public interface IRabbitMQPublisher
    {
        Task PublishMessageAsync<T>(T message, string queueName);
        Task SendStkResponseMessage<T>(T message);
    }
}
