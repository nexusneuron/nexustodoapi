using MassTransit.Contracts;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Channels;

namespace TodoAPI.MessageBroker.Services
{
    public class RabbitMQConsumer : IRabbitMQConsumer
    {
        private readonly RabbitMQSetting _rabbitMqSetting;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;

        public RabbitMQConsumer(IOptions<RabbitMQSetting> rabbitMqSetting, IRabbitMQPublisher rabbitMQPublisher)
        {
            _rabbitMqSetting = rabbitMqSetting.Value;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        //public async Task ConsumeMessageAsync<T>(T message, string queueName)
        public async Task ConsumeMessageAsync(string queueName)
        {

            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqSetting.HostName,
                UserName = _rabbitMqSetting.UserName,
                Password = _rabbitMqSetting.Password,
                Port = _rabbitMqSetting.Port
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);


            Console.WriteLine("QUEUE DECLARED");
            Console.WriteLine(queueName);
            ////
            ///
            /// 
            ///

            var consumer = new AsyncEventingBasicConsumer(channel);

            Console.WriteLine("Reached INSIDE");

            //if (consumer.Channel.MessageCountAsync(queueName).IsCompletedSuccessfully)
            //{
                //consumer.ReceivedAsync += async (model, ea) =>
                //consumer.ReceivedAsync += async (model, ea) =>
                //{
                    //var body = ea.Body.ToArray();
                    //var message = Encoding.UTF8.GetString(body);

                    //Console.WriteLine("Read from Queue INSIDE");

                    //Console.WriteLine(message);


                    // Send an acknowledgement to RabbitMQ
                    //await channel.BasicAckAsync(ea.DeliveryTag, false);


                    //bool processedSuccessfully = false;
                    //try
                    //{

                    //processedSuccessfully = await JsonConvert.DeserializeObject<dynamic>(message);
                    //}
                    //catch (Exception ex)
                    //{
                    //    //_logger.LogError($"Exception occurred while processing message from queue {queueName}: {ex}");
                    //    Console.WriteLine("Exception occurred while processing message from queue {queueName}: {ex}");
                    //}

                    //if (processedSuccessfully)
                    //{
                        //await _rabbitMQPublisher.PublishMessageAsync(message, "queueTitle.QueueTitle");
                        //await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    //}
                    //else
                    //{
                    //    await channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: true);
                    //}
                //};

                //await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

            //}
            //else
            //{
            //    Console.WriteLine("Exception No message. No Queue");
            //}

            ////consumer.ReceivedAsync += async (model, ea) =>
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine("Read from Queue INSIDE");

				Console.WriteLine(message);


            //    // Send an acknowledgement to RabbitMQ
                await channel.BasicAckAsync(ea.DeliveryTag, false);


            //    bool processedSuccessfully = false;
            //    try
            //    {

            //        processedSuccessfully = await JsonConvert.DeserializeObject<dynamic>(message);
            //    }
            //    catch (Exception ex)
            //    {
            //        //_logger.LogError($"Exception occurred while processing message from queue {queueName}: {ex}");
            //        Console.WriteLine("Exception occurred while processing message from queue {queueName}: {ex}");
            //    }

            //    if (processedSuccessfully)
            //    {
            //        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            //    }
            //    else
            //    {
            //        await channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: true);
            //    }
            //};

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

        }

    }
}

