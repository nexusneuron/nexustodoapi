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
        public async Task<callbackresponse> ConsumeMessageAsync(string queueName, string merchantID)
        {
            bool value = false;
            string value2 = string.Empty;
            callbackresponse _callbackresponse = new callbackresponse();

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

            //if(status.MessageCount == 0)


            Console.WriteLine("//////////////////////////////////////////////////////");
            Console.WriteLine("QUEUE DECLARED");
            Console.WriteLine(queueName);
            ////

            var consumer = new AsyncEventingBasicConsumer(channel);
            //if (consumer.Channel.MessageCountAsync(queueName) != null)

            if (status.MessageCount > 0)
            {
                Console.WriteLine("Channel 1 queue has Message");
                Console.WriteLine("//////////////////////////////////////////////////////");

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);


                    //Deserialize Classes
                    RootCallback requestCallback = JsonConvert.DeserializeObject<RootCallback>(message);

                    RootConfirmation requestConfirmation = JsonConvert.DeserializeObject<RootConfirmation>(message);

                    //Message is from Confirmation URL
                    if (requestCallback.Body.stkCallback == null)
                    {
                        Console.WriteLine("//////////////////////////////////////////////////////");
                        Console.WriteLine("Message is from Confirmation URL");

                        //Deserialize RootConfirmation Display this message only
                        Console.WriteLine(requestConfirmation.FirstName);
                        Console.WriteLine("//////////////////////////////////////////////////////");

                        bool value = true;

                        string value2 = message;
                    }
                    else
                    {
                        Console.WriteLine("//////////////////////////////////////////////////////");
                        //Deserialized RootCallback Display this message only
                        Console.WriteLine("Message is from Callback URL");
                        Console.WriteLine(requestCallback.Body.stkCallback.CheckoutRequestID);
                        Console.WriteLine("//////////////////////////////////////////////////////");

                        bool value = true;

                        string value2 = message;
                    }


                    //    // Send an acknowledgement to RabbitMQ
                    await channel.BasicAckAsync(ea.DeliveryTag, false);


                };

                //callback or confirmation was successful
                await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);


                _callbackresponse.Message = value2;
                _callbackresponse.value = value;

                Console.WriteLine("//////////////////////////////////////////////////////");
                Console.WriteLine("//////////////////////////////////////////////////////" + " " + _callbackresponse.Message + "///////" + " " + _callbackresponse.value.ToString());
                Console.WriteLine("//////////////////////////////////////////////////////");




                //return response to confirmpayment
                return _callbackresponse;

            }
            //else if(consumer.Channel.MessageCountAsync(queueName) == null ) ////RETRYING
            //else if (status.MessageCount == 0)
            //{
            //    await Task.Delay(30 * 1000);

            //    Console.WriteLine("Channel 1 queue has Message  RETRIED");
            //    Console.WriteLine("//////////////////////////////////////////////////////");

            //    //if (consumer.Channel.MessageCountAsync(queueName) != nullstatus.MessageCount > 0)
            //    if (status.MessageCount > 0)
            //    {

            //        consumer.ReceivedAsync += async (model, ea) =>
            //        {
            //            var body = ea.Body.ToArray();
            //            var message = Encoding.UTF8.GetString(body);


            //            //Deserialize Classes
            //            RootCallback requestCallback = JsonConvert.DeserializeObject<RootCallback>(message);

            //            RootConfirmation requestConfirmation = JsonConvert.DeserializeObject<RootConfirmation>(message);

            //            if (message != null)
            //            {
            //                //Message is from Confirmation URL
            //                if (requestCallback.Body.stkCallback == null)
            //                {
            //                    Console.WriteLine("//////////////////////////////////////////////////////");
            //                    Console.WriteLine("Message is from Confirmation URL");

            //                    //Deserialize RootConfirmation Display this message only
            //                    Console.WriteLine(requestConfirmation.FirstName);
            //                    Console.WriteLine("//////////////////////////////////////////////////////");

            //                    bool value = true;

            //                    string value2 = message;
            //                }
            //                else
            //                {
            //                    Console.WriteLine("//////////////////////////////////////////////////////");
            //                    //Deserialized RootCallback Display this message only
            //                    Console.WriteLine("Message is from Callback URL");
            //                    Console.WriteLine(requestCallback.Body.stkCallback.CheckoutRequestID);
            //                    Console.WriteLine("//////////////////////////////////////////////////////");

            //                    bool value = true;

            //                    string value2 = message;
            //                }

            //                //    // Send an acknowledgement to RabbitMQ
            //                await channel.BasicAckAsync(ea.DeliveryTag, false);

            //            }

            //        };

            //        //callback or confirmation was successful
            //        await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

            //        _callbackresponse.Message = value2;
            //        _callbackresponse.value = value;

            //        Console.WriteLine("//////////////////////////////////////////////////////");
            //        Console.WriteLine("//////////////////////////////////////////////////////" + " " + _callbackresponse.Message + "///////" + " " + _callbackresponse.value.ToString());
            //        Console.WriteLine("//////////////////////////////////////////////////////");


            //        //return response to confirmpayment
            //        return _callbackresponse;

            //    }

            //    _callbackresponse.Message = value2;
            //    _callbackresponse.value = value; 

            //    return _callbackresponse;

            //}
            //else if (consumer.Channel.MessageCountAsync(queueName) == null) ////RETRYING   CHANNEL 2//
            else if (status.MessageCount == 0)
            {
                Console.WriteLine("Channel 2 NEEDS TO BE CREATED");
                Console.WriteLine("//////////////////////////////////////////////////////");

                Console.WriteLine("//////////////////////////////////////////////////////");
                Console.WriteLine("CONSUMING MESSAGE CONSTRUCTOR 2.    DELAY 15sec  error MESSAGE TO BE QUEUED");
                Console.WriteLine("//////////////////////////////////////////////////////");

                await Task.Delay(15 * 1000);

                //check queue based on merchantID
                using var channel2 = await connection.CreateChannelAsync();
                //use merchantID as queueName
                var status2 = await channel2.QueueDeclareAsync(queue: merchantID, durable: false, exclusive: false, autoDelete: false, arguments: null);

                if (status2.MessageCount > 0)
                    Console.WriteLine("//////////////////////////////////////////////////////");
                    Console.WriteLine("FOUND ERROR QUEUE EMPTY.    DELAY 15sec");
                    Console.WriteLine("//////////////////////////////////////////////////////");
                await Task.Delay(15 * 1000) ;

                Console.WriteLine("QUEUE 2 DECLARED");
                var consumer2 = new AsyncEventingBasicConsumer(channel2);



                consumer2.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine("Read from Queue2 INSIDE");
                    Console.WriteLine(message);


                    Console.WriteLine("//////////////////////////////////////////////////////");
                    Console.WriteLine("Message is from merchantID error");

                    //Deserialize RootConfirmation Display this message only
                    Console.WriteLine(message);
                    Console.WriteLine("//////////////////////////////////////////////////////");

                    bool value = false;

                    string value2 = message;


                    //    // Send an acknowledgement to RabbitMQ
                    await channel2.BasicAckAsync(ea.DeliveryTag, false);

                    //if (message != null)
                    //{
                    //    Console.WriteLine("//////////////////////////////////////////////////////");
                    //    Console.WriteLine("Message is from merchantID error");

                    //    //Deserialize RootConfirmation Display this message only
                    //    Console.WriteLine(message);
                    //    Console.WriteLine("//////////////////////////////////////////////////////");

                    //    bool value = false;

                    //    string value2 = message;


                    //    //    // Send an acknowledgement to RabbitMQ
                    //    await channel2.BasicAckAsync(ea.DeliveryTag, false);


                    //    //return response to confirmpayment
                    //}
                    //else if (message == null)  //retrying TO CHECK MERCHANTID ERROR MESSAGE
                    //{
                    //    //DELAY 15 Sec  MESSAGE TO BE retried
                    //    await Task.Delay(15 * 1000);

                    //    Console.WriteLine("//////////////////////////////////////////////////////");
                    //    var message2 = Encoding.UTF8.GetString(body);
                    //    Console.WriteLine("//////////////////////////////////////////////////////" + " " + message2);

                    //    //Message is from merchantID error
                    //    //if (message != null)
                    //    try
                    //    {
                    //        Console.WriteLine("//////////////////////////////////////////////////////");
                    //        Console.WriteLine("Message is from merchantID error" + message2);
                    //        Console.WriteLine("//////////////////////////////////////////////////////");

                    //        bool value = false;
                    //        string value2 = message2;

                    //        //    // Send an acknowledgement to RabbitMQ
                    //        await channel2.BasicAckAsync(ea.DeliveryTag, false);

                    //        //return response to confirmpayment about error

                    //    }
                    //    catch (Exception ex)
                    //    //else
                    //    {
                    //        Console.WriteLine("//////////////////////////////////////////////////////");
                    //        //Deserialized RootCallback Display this message only
                    //        Console.WriteLine("Message is from merchantID error STK FAILED & ERROR FROM merchantID unknown");
                    //        Console.WriteLine(ex.Message.ToString());
                    //        Console.WriteLine("//////////////////////////////////////////////////////");

                    //        bool value = false;
                    //        string value2 = "ERROR FROM merchantID unknown";
                    //    }


                    //    //    // Send an acknowledgement to RabbitMQ
                    //    await channel2.BasicAckAsync(ea.DeliveryTag, false);


                    //    //return response to confirmpayment
                    //}


                };

                //Callback has error after execution
                await channel2.BasicConsumeAsync(queue: merchantID, autoAck: false, consumer: consumer2);


                _callbackresponse.Message = value2;
                _callbackresponse.value = value;

                Console.WriteLine("//////////////////////////////////////////////////////     BLOCK 1");
                Console.WriteLine("//////////////////////////////////////////////////////" + " " + _callbackresponse.Message + "///////" + " " + _callbackresponse.value.ToString());
                Console.WriteLine("//////////////////////////////////////////////////////");

                return _callbackresponse;
            }

            _callbackresponse.Message = value2;
            _callbackresponse.value = value;

            Console.WriteLine("//////////////////////////////////////////////////////     BLOCK 2");
            Console.WriteLine("//////////////////////////////////////////////////////" + " " + _callbackresponse.Message + "///////" + " " + _callbackresponse.value.ToString());
            Console.WriteLine("//////////////////////////////////////////////////////");

            return _callbackresponse;
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