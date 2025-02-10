using Microsoft.AspNetCore.Mvc;

namespace TodoAPI.MessageBroker.Services
{
    public interface IRabbitMQConsumer
    {
        //Task ConsumeMessageAsync<T>(T message, string queueName);
        //Task ConsumeMessageAsync(string queueName);
        //Task<callbackresponse> ConsumeMessageAsync(string queueName, string merchantID);
        Task<bool> ConsumeMessageAsync(string queueName, string merchantID);

        public class callbackresponse
        {
            public string Message { get; set; }
            public bool value { get; set; }
        }
    }
}
