using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace TodoAPI.MessageBroker.Services
{
    //public class RabbitMQPublisher<T> : IRabbitMQPublisher<T>
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly RabbitMQSetting _rabbitMqSetting;

        public RabbitMQPublisher(IOptions<RabbitMQSetting> rabbitMqSetting)
        {
            _rabbitMqSetting = rabbitMqSetting.Value;
        }

        public async Task PublishMessageAsync<T>(T message, string queueName)
        {

            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqSetting.HostName,
                UserName = _rabbitMqSetting.UserName,
                Password = _rabbitMqSetting.Password
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var messageJson = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageJson);

            await Task.Run(() => channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body));
        }

        public async void SendStkResponseMessage<T>(T message)
        {
            //Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
            //var factory = new ConnectionFactory
            //{
            //    HostName = "localhost"
            //};

            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqSetting.HostName,
                UserName = _rabbitMqSetting.UserName,
                Password = _rabbitMqSetting.Password
            };

            //Create the RabbitMQ connection using connection factory details as i mentioned above
            //var connection = factory.CreateConnection();

            //Here we create channel with session and model
            //using var channel = connection.CreateModel();

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            //declare the queue after mentioning name and a few property related to that
            //channel.QueueDeclare("product", exclusive: false);
            await channel.QueueDeclareAsync(queue: "todoitem", durable: false, exclusive: false, autoDelete: false, arguments: null);

            //Serialize the message
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            //put the data on to the product queue
            //channel.BasicPublish(exchange: "", routingKey: "product", body: body);
            await channel.BasicPublishAsync(exchange: "", routingKey: "todoitem", body: body);
        }
    }
}
