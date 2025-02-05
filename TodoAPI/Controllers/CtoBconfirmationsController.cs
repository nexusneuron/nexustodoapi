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


            Console.WriteLine("i have hit ctob");
            Console.WriteLine(body);


            // As well as a bound model
            //var request = JsonConvert.DeserializeObject<StkCallbackPartial>(bodyjson);
            Root request = JsonConvert.DeserializeObject<Root>(body);


            if (request == null)
            {
                //Console.WriteLine("request came back empty");

                //return unsuccesful to user
                //log error

                return NoContent();
            }


            TodoAPI.Models.NexuspayConfirmation ctobresponse = new TodoAPI.Models.NexuspayConfirmation()
            {
                TransAmount = request.TransAmount,
                BillRefNumber = request.BillRefNumber,
                TransID = request.TransID,
                TransTime = request.TransTime,
                FirstName = request.FirstName,
                OrgAccountBalance = request.OrgAccountBalance,
                BusinessShortCode = request.BusinessShortCode,
                MSISDN  = request.MSISDN,
                TransactionType = request.TransactionType,
            };


            //DB
            _context.NexuspayConfirmation.Add(ctobresponse);
            await _context.SaveChangesAsync();


            //encode amount + TRANSTIME
            //ctobresponse.TransAmount   float  to  truncated zero decimal int then string
            int i = (int)float.Truncate(float.Parse(ctobresponse.TransAmount));
            string amount = i.ToString();
            string TransTime = ctobresponse.TransTime.ToString();


            byte[] _amtTime = Encoding.UTF8.GetBytes(amount + TransTime);
            String _encodedamtTime = System.Convert.ToBase64String(_amtTime);


            // publish stk response based on amt & transtime
            RabbitMQQueues queueTitle2 = new RabbitMQQueues();
            queueTitle2.QueueTitle = _encodedamtTime;
            await _rabbitMQPublisher.PublishMessageAsync(request, queueTitle2.QueueTitle);

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
