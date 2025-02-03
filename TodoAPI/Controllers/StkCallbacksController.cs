using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
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
            //var request = JsonConvert.DeserializeObject<StkCallbackPartial>(bodyjson);
            Root request = JsonConvert.DeserializeObject<Root>(body);

            if(request.Body.stkCallback.ResultCode != 0)
            {
                Console.WriteLine(request.Body.stkCallback.ResultDesc);

                //return unsuccesful to user
                //log error

                return NoContent();
            }

            Console.WriteLine(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("MpesaReceiptNumber")).Value);

            TodoAPI.Models.StkCallback stkresponse = new TodoAPI.Models.StkCallback()
            {
                //AccountReference =   TransactionDate from model //Account number  //check if amts under this account reference total with invoice
                Amount = int.Parse(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("Amount")).Value.ToString()),
                CheckoutRequestID = request.Body.stkCallback.CheckoutRequestID,
                MerchantRequestID = request.Body.stkCallback.MerchantRequestID,
                MpesaReceiptNumber = request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("MpesaReceiptNumber")).Value.ToString(),
                //Name = jsonrespo.Name,
                PhoneNumber = long.Parse(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("PhoneNumber")).Value.ToString()),
                //TransactionDate = TransactionDate from model,
                //TransactionDesc = TransactionDesc from model,
            };

            //Console.WriteLine(value);
            //Console.WriteLine(jsonrespo);
            Console.WriteLine(stkresponse.AccountReference);


            //DB
            _context.SktCallback.Add(stkresponse);
            await _context.SaveChangesAsync();

            //encode phone + amount //PHONE+MERCHANTREQUESTDID
            int amount = int.Parse(stkresponse.Amount.ToString());
            string phone = stkresponse.PhoneNumber.ToString();
            string merchantID = stkresponse.MerchantRequestID;
           
            //string accNO = "CompanyXLTD";
            string accNO = "Nexuspay Initial";


            byte[] _amtAccNo = Encoding.UTF8.GetBytes(amount + accNO);
            String _encodedamtAccNo = System.Convert.ToBase64String(_amtAccNo);


            RabbitMQQueues queueTitle = new RabbitMQQueues();
            //queueTitle.QueueTitle = request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("MpesaReceiptNumber")).Value.ToString();
            queueTitle.QueueTitle = _encodedamtAccNo;


            // publish order validation data
            //await _rabbitMQPublisher.PublishMessageAsync(request.Body, RabbitMQQueues.stkcallbackqueue);
            await _rabbitMQPublisher.PublishMessageAsync(request.Body, queueTitle.QueueTitle);

            //await _rabbitMQConsumer.ConsumeMessageAsync(queueTitle.QueueTitle);

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
