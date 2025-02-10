using MassTransit.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Security.Policy;
using System.Text;
using System.Threading.Channels;
using static MassTransit.Util.RequestRateAlgorithm;
using static TodoAPI.MessageBroker.Services.IRabbitMQConsumer;

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


        //Callback deserialize classes
        public class RootCallback
        {
            public Body Body { get; set; }
        }

        public class Body
        {
            public StkCallback stkCallback { get; set; }
        }

        public class CallbackMetadata
        {
            public List<Item> Item { get; set; }
        }

        public class Item
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        public class StkCallback
        {
            public string MerchantRequestID { get; set; }
            public string CheckoutRequestID { get; set; }
            public int ResultCode { get; set; }
            public string ResultDesc { get; set; }
            public CallbackMetadata CallbackMetadata { get; set; }
        }


        //CtoBConfirmation deserialize classes
        public class RootConfirmation
        {
            public string TransactionType { get; set; }
            public string TransID { get; set; }
            public string TransTime { get; set; }
            public string TransAmount { get; set; }
            public string BusinessShortCode { get; set; }
            public string BillRefNumber { get; set; }
            public string InvoiceNumber { get; set; }
            public string OrgAccountBalance { get; set; }
            public string ThirdPartyTransID { get; set; }
            public string MSISDN { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
        }

        //public class callbackresponse
        //{
        //    public string Message { get; set; }
        //    public bool value { get; set; }
        //}

        //public async Task ConsumeMessageAsync<T>(T message, string queueName)
        //public async Task<callbackresponse> ConsumeMessageAsync(string queueName, string merchantID)

        //public async Task<bool> ConsumeMessageAsync(string queueName, string merchantID)
        public async Task<bool> ConsumeMessageAsync(string queueName, string merchantID)
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqSetting.HostName,
                UserName = _rabbitMqSetting.UserName,
                Password = _rabbitMqSetting.Password,
                Port = _rabbitMqSetting.Port
            };

            Console.WriteLine("//////////////////////////////////////////////////////");
            Console.WriteLine("CONSUMING MESSAGE CONSTRUCTOR.  15secs  DELAY  MINUTE  MESSAGE TO BE QUEUED");
            Console.WriteLine("//////////////////////////////////////////////////////");
            //DELAY 0.5 MINUTE  MESSAGE TO BE QUEUED      B4 CONSUMING  CONSTRUCTING QUEUE CONN

            await Task.Delay(15 * 1000);

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            var status = await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var status2 = await channel.QueueDeclareAsync(queue: merchantID, durable: false, exclusive: false, autoDelete: false, arguments: null);
            //if(status.MessageCount == 0)


            Console.WriteLine("//////////////////////////////////////////////////////");
            Console.WriteLine("QUEUE DECLARED");
            Console.WriteLine(queueName);
            ////

            var consumer = new AsyncEventingBasicConsumer(channel);
            //if (consumer.Channel.MessageCountAsync(queueName) != null)

            bool value = false;

            try
            {

                if (status.MessageCount > 0)
                {
                    Console.WriteLine("Channel 1 queue has Message");
                    Console.WriteLine("//////////////////////////////////////////////////////");

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);


                        Console.WriteLine("//////////////////////////////////////////////////////        Channel 1 queue has Message");
                        Console.WriteLine(message);

                        //Deserialize Classes
                        RootCallback requestCallback = JsonConvert.DeserializeObject<RootCallback>(message);

                        RootConfirmation requestConfirmation = JsonConvert.DeserializeObject<RootConfirmation>(message);

                        //Message is from Confirmation URL
                        Console.WriteLine("//////////////////////////////////////////////////////");
                        Console.WriteLine("Message is from Confirmation URL");
                        //Deserialize RootConfirmation Display this message only
                        Console.WriteLine(requestConfirmation.FirstName);
                        Console.WriteLine("//////////////////////////////////////////////////////");


                        Console.WriteLine("//////////////////////////////////////////////////////");
                        //Deserialized RootCallback Display this message only
                        Console.WriteLine("Message is from Callback URL");
                        Console.WriteLine(requestCallback.Body.stkCallback.CheckoutRequestID);
                        Console.WriteLine("//////////////////////////////////////////////////////");

                        //    // Send an acknowledgement to RabbitMQ
                        await channel.BasicAckAsync(ea.DeliveryTag, false);


                    };

                    //callback or confirmation was successful
                    await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

                    value = true;

                    //return response to confirmpayment
                    //return _callbackresponse;
                    return (value);               

                }
                else if (status2.MessageCount > 0)
                {
                    Console.WriteLine("Channel 2 NEEDS TO BE CREATED");
                    Console.WriteLine("//////////////////////////////////////////////////////");

                    Console.WriteLine("//////////////////////////////////////////////////////");
                    Console.WriteLine("CONSUMING MESSAGE CONSTRUCTOR 2.    DELAY 15sec  error MESSAGE TO BE QUEUED");
                    Console.WriteLine("//////////////////////////////////////////////////////");

                    await Task.Delay(15 * 1000);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        Console.WriteLine("//////////////////////////////////////////////////////");
                        Console.WriteLine("Message is from merchantID error");

                        //Deserialize RootConfirmation Display this message only
                        Console.WriteLine(message);
                        Console.WriteLine("//////////////////////////////////////////////////////");

                        /// Send an acknowledgement to RabbitMQ
                        //await channel2.BasicAckAsync(ea.DeliveryTag, false);
                        await channel.BasicAckAsync(ea.DeliveryTag, false);

                    };

                    //Callback has error after execution
                    //await channel2.BasicConsumeAsync(queue: merchantID, autoAck: false, consumer: consumer2);
                    await channel.BasicConsumeAsync(queue: merchantID, autoAck: false, consumer: consumer);


                    Console.WriteLine("//////////////////////////////////////////////////////     BLOCK 1");
                
                    value = false;

                    //return _callbackresponse;
                    return (value);
                }
            }
            catch (Exception ex)
            {

            }

            return (value);
        }

    }
}








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