using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using TodoAPI.MessageBroker;
using TodoAPI.MessageBroker.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CtoBconfirmationsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;

        public CtoBconfirmationsController(ApplicationDbContext context, IRabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        // GET: api/<CtoBconfirmationsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CtoBconfirmationsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        public class Root
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

        // POST api/<CtoBconfirmationsController>
        [HttpPost]
        //public void Post([FromBody] string value)
        public async Task<IActionResult> Post()
        {

            using var reader = new StreamReader(HttpContext.Request.Body);

            // You now have the body string raw
            var body = await reader.ReadToEndAsync();


            Console.WriteLine(body);


            // As well as a bound model
            //var request = JsonConvert.DeserializeObject<StkCallbackPartial>(bodyjson);
            Root request = JsonConvert.DeserializeObject<Root>(body);

            if (request == null)
            {
                Console.WriteLine("request came back empty");

                //return unsuccesful to user
                //log error

                return NoContent();
            }

            Console.WriteLine(request.InvoiceNumber);


            TodoAPI.Models.NexuspayConfirmation ctobresponse = new TodoAPI.Models.NexuspayConfirmation()
            {
                TransAmount = int.Parse(request.TransAmount.ToString()),
                BillRefNumber = request.BillRefNumber,
                //AccountReference =   TransactionDate from model //Account number  //check if amts under this account reference total with invoice
                //Amount = int.Parse(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("Amount")).Value.ToString()),
                //CheckoutRequestID = request.Body.stkCallback.CheckoutRequestID,
                //MpesaReceiptNumber = request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("MpesaReceiptNumber")).Value.ToString(),
                //Name = jsonrespo.Name,
                //PhoneNumber = long.Parse(request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("PhoneNumber")).Value.ToString()),
                //TransactionDate = TransactionDate from model,
                //TransactionDesc = TransactionDesc from model,
            };


            Console.WriteLine(ctobresponse.TransID);


            //DB
            _context.NexuspayConfirmation.Add(ctobresponse);
            await _context.SaveChangesAsync();


            //encode phone + amount //PHONE+MERCHANTREQUESTDID
            int amount = int.Parse(ctobresponse.TransAmount.ToString());
            //string phone = ctobresponse.PhoneNumber.ToString();
            string accNO = ctobresponse.BillRefNumber;

            byte[] _amtAccNo = Encoding.UTF8.GetBytes(amount + accNO);
            String _encodedamtAccNo = System.Convert.ToBase64String(_amtAccNo);


            RabbitMQQueues queueTitle = new RabbitMQQueues();
            //queueTitle.QueueTitle = request.Body.stkCallback.CallbackMetadata.Item.Find(r => r.Name.Equals("MpesaReceiptNumber")).Value.ToString();
            queueTitle.QueueTitle = _encodedamtAccNo;


            // publish order validation data
            //await _rabbitMQPublisher.PublishMessageAsync(request.Body, RabbitMQQueues.stkcallbackqueue);
            await _rabbitMQPublisher.PublishMessageAsync(request, queueTitle.QueueTitle);

            //await _rabbitMQConsumer.ConsumeMessageAsync(queueTitle.QueueTitle);

            return NoContent();
        }


        // PUT api/<CtoBconfirmationsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CtoBconfirmationsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
