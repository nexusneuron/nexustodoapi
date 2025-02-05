﻿using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json;
using NuGet.Protocol;
using RestSharp;
using System.Text;
using System.Text.Json.Nodes;
using TodoAPI.MessageBroker;
using TodoAPI.MessageBroker.Services;
using static TodoAPI.Controllers.StkCallbacksController;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StkCallbacksController : ControllerBase
    {
        public readonly ApplicationDbContext _context;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;

        public StkCallbacksController(ApplicationDbContext context, IRabbitMQPublisher rabbitMQPublisher) 
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }


        // GET: api/<StkCallbacksController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<StkCallbacksController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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

        public class Root
        {
            public Body Body { get; set; }
        }

        public class StkCallback
        {
            public string MerchantRequestID { get; set; }
            public string CheckoutRequestID { get; set; }
            public int ResultCode { get; set; }
            public string ResultDesc { get; set; }
            public CallbackMetadata CallbackMetadata { get; set; }
        }



        // POST api/<StkCallbacksController>
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);

            // You now have the body string raw
            var body = await reader.ReadToEndAsync();


            Console.WriteLine(body);


            // As well as a bound model
            Root request = JsonConvert.DeserializeObject<Root>(body);

            //[used to check if a queue exists with the merchantID cos there was an error stk not processed successfuly. logs error.  advises user in display]
            if (request.Body.stkCallback.ResultCode != 0)
            {

                Console.WriteLine(request.Body.stkCallback.ResultDesc);

                var merchantID = request.Body.stkCallback.MerchantRequestID;

                // publish stk error response based on merchantID
                RabbitMQQueues queueTitle = new RabbitMQQueues();
                queueTitle.QueueTitle = merchantID;

                //CREATE queue with the merchantID  MESSAGE IS ERROR
                //TO BE CONSUMED BY CONFIRMPAYMENT METHOD 
                await _rabbitMQPublisher.PublishMessageAsync(request.Body, queueTitle.QueueTitle);

                //return unsuccesful to user
                //log error

                return NoContent();
            }

            Console.WriteLine(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("MpesaReceiptNumber")).Value);


            //encode amount + TRANSTIME
            int amount = int.Parse(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("Amount")).Value.ToString());
            string TransTime = request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("TransactionDate")).Value.ToString();


            byte[] _amtTime = Encoding.UTF8.GetBytes(amount + TransTime);
            String _encodedamtTime = System.Convert.ToBase64String(_amtTime);


            //GET ACC NO & other details FROM TEMPORARY TABLE
            //Add DATA TO call back table into 
            var tempitem = _context.TempSTKData.First(n => n.AmtTime == _encodedamtTime);

            TodoAPI.Models.StkCallback stkresponse = new TodoAPI.Models.StkCallback()
            {
                AccountReference = tempitem.accNO,
                Amount = int.Parse(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("Amount")).Value.ToString()),
                CheckoutRequestID = request.Body.stkCallback.CheckoutRequestID,
                MerchantRequestID = request.Body.stkCallback.MerchantRequestID,
                MpesaReceiptNumber = request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("MpesaReceiptNumber")).Value.ToString(),
                Name = "username logged",
                PhoneNumber = long.Parse(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("PhoneNumber")).Value.ToString()),
                TransactionDate = request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("TransactionDate")).Value.ToString(),
                TransactionDesc = tempitem.TransactionDesc,
            };


            //DB
            _context.SktCallback.Add(stkresponse);
            await _context.SaveChangesAsync();


            ////DB  change bool of PAID for  INV TABLE BASED ON ACCNO FROM TEMPRARY TABLE  to true
            //_context.SktCallback.Add(stkresponse);
            //await _context.SaveChangesAsync();


            //DB  DELETE ANY  FROM TEMPRARY TABLE  BASED ON THE REF ACCNO
            var tempstkitems = _context.TempSTKData.Where(n => n.accNO == tempitem.accNO);
            foreach (TempSTKData n in tempstkitems)
            {
                _context.TempSTKData.Remove(n);
            }

            await _context.SaveChangesAsync();



            // publish stk response based on amt & transtime
            RabbitMQQueues queueTitle2 = new RabbitMQQueues();
            queueTitle2.QueueTitle = _encodedamtTime;
            await _rabbitMQPublisher.PublishMessageAsync(request.Body, queueTitle2.QueueTitle);


            return NoContent();
        }

        // PUT api/<StkCallbacksController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<StkCallbacksController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
