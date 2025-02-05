using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using TodoAPI.Interfaces;
using TodoAPI.MessageBroker;
using TodoAPI.MessageBroker.Services;
using TodoAPI.Models;
using TodoAPI.Services;

namespace TodoAPI.Controllers
{
    #region snippetErrorCode
    public enum ErrorCode
    {
        TodoItemNameAndNotesRequired,
        TodoItemIDInUse,
        RecordNotFound,
        CouldNotCreateItem,
        CouldNotUpdateItem,
        CouldNotDeleteItem
    }
    #endregion

    #region snippetDI
    [ApiController]
    [Route("api/[controller]")]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoRepository _todoRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMpesaServices _mpesaservice;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;
        private readonly IRabbitMQConsumer _rabbitMQConsumer;

        public TodoItemsController(ITodoRepository todoRepository, ApplicationDbContext context, IMpesaServices mpepe, IRabbitMQPublisher rabbitMQPublisher, IRabbitMQConsumer rabbitMQConsumer)
        {
            _todoRepository = todoRepository;
            _context = context;
            _mpesaservice = mpepe;
            _rabbitMQPublisher = rabbitMQPublisher;
            _rabbitMQConsumer = rabbitMQConsumer;
        }
        #endregion

        #region snippet
        [HttpGet]
        public IActionResult List()
        {
            //return Ok(_todoRepository.All);
            return Ok(_context.TodoItems.ToList());
        }
        #endregion

        public class Root
        {
            public string MerchantRequestID { get; set; }
            public string CheckoutRequestID { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseDescription { get; set; }
            public string CustomerMessage { get; set; }
        }

        //Read message from Message Queue
        public async Task<IActionResult> ConfirmPayment(string _encodedamtTime, string merchantID)
        {
            Console.WriteLine("Reading from messaging QUEUE after saving to DB.    DELAY 1 MINUTE   USER TO EXECUTE PIN INPUT");

            //DELAY 1 MINUTE   USER TO EXECUTE PIN INPUT      B4 CONSUMING   QUEUE
            await Task.Delay(60 * 1000);

            var response = await _rabbitMQConsumer.ConsumeMessageAsync(_encodedamtTime, merchantID);

            if(response.value == true)
            {

                Console.WriteLine(response.Message.ToString());
                return Ok(response);

                //
            }


            Console.WriteLine(response.Message.ToString());
            return BadRequest(response);
            //user to retry

            Console.WriteLine("Read from ConfirmPayment method");
        }


        #region snippetCreate
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]TodoItem item)
        {
            try
            {
                if (item == null || !ModelState.IsValid)
                {
                    return BadRequest(ErrorCode.TodoItemNameAndNotesRequired.ToString());
                }
                bool itemExists = _todoRepository.DoesItemExist(item.ID);
                if (itemExists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, ErrorCode.TodoItemIDInUse.ToString());
                }

                //_todoRepository.Insert(item);
                //item.ID = new Guid().ToString();


                //mpesaservice oauth();
                //var response = await _mpesaservice.oauth2();
                //Console.WriteLine(response.Content);


                ////CtoBSimulate
                //var ctobresponse = await _mpesaservice.c2bsimulate();
                ////CtoBRegisterURL
                //var ctobresponse = await _mpesaservice.CtoBRegisterURL();
                //Console.WriteLine(ctobresponse.Content);


                ////STK Push OBJECTS
                DateTime d = DateTime.Now;
                string dateString = d.ToString("yyyyMMddHHmmss");


                //encode amt + TransTime
                int businessShortcode = 5142254;
                int amount = 1;
                long partyA = 254717904391;
                //string accNO = "CompanyXLTD";
                string accNO = "NexuspayIni";  //VALUE FROM INV/ORDERS TABLE
                string TransTime = dateString;
                int PartyB = 5142254;
                long PhoneNumber = 254717904391;
                //CallBackURL = "https://buzzard-hip-donkey.ngrok-free.app/api/stkcallbacks";
                string CallBackURL = "https://nexuspay.nexusneuron.com/api/stkcallbacks";
                string TransactionDesc = "NexuspayPro";
                string passkey = "6b4ef1cdca85cb784b049776727e927d73dcdcd717444e51a53e0e7579e5dad6";

                ///  PASS
                ///  MULTITENANT [ PASS   PASSKEY, BusinessShortCode = shortcode,  Amount = 1, PartyA = 254717904391, PartyB = SHORTCODE/TILLNUMBER, PhoneNumber = 254717904391, CallBackURL = "https://nexuspay.nexusneuron.com/api/stkcallbacks", AccountReference = "NexuspayIni",TransactionDesc = "NexuspayPro" ]
                //var response = await _mpesaservice.stkpush();

                var response = await _mpesaservice.stkpush(businessShortcode, amount, partyA, accNO, TransTime, PartyB, PhoneNumber, CallBackURL,  TransactionDesc, passkey);


				Console.WriteLine("expectin response read within todoitems controller    IF HAS ERROR USER IS NOTIFIED TO RETRY IN A MIN");
				if (response.ErrorException != null)
				{
					Console.WriteLine(response.ErrorException.Message);
					Console.WriteLine(response.ErrorMessage);
					Console.WriteLine(response.StatusCode);
					Console.WriteLine(response.Content);
				}

                //Stream reader RAW FROM HTTP REQUEST
                //using var reader = new StreamReader(HttpContext.Request.Body);
                // You now have the body string raw
                //var body = await reader.ReadToEndAsync();


                //Get merchant ID
                Root requestResponse = JsonConvert.DeserializeObject<Root>(response.Content);
                String merchantID = requestResponse.MerchantRequestID;



                byte[] _amtTime = Encoding.UTF8.GetBytes(amount + TransTime);
                String _encodedamtTime = System.Convert.ToBase64String(_amtTime);


                ////DB TEMPORARY DATA TABLE
                TempSTKData tempsdkdata = new TempSTKData() 
                {
                    AmtTime = _encodedamtTime,
                    businessShortcode = businessShortcode,
                    amount = amount,
                    partyA = partyA,
                    accNO = accNO,
                    TransTime = TransTime,
                    PartyB = PartyB,
                    PhoneNumber = PhoneNumber,
                    CallBackURL = CallBackURL,
                    TransactionDesc = TransactionDesc,
                };

                _context.TempSTKData.Add(tempsdkdata);
                await _context.SaveChangesAsync();


                ////DB TODOITEMS
                _context.TodoItems.Add(item);
                await _context.SaveChangesAsync();


                Console.WriteLine("Calling ConfirmPayment method");

                try
                {
                    //Calling ConfirmPayment method
                    ConfirmPayment(_encodedamtTime, merchantID);
                }
                catch (Exception)
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ErrorCode.CouldNotCreateItem.ToString());
            }
            return Ok(item);
        }
        #endregion

        #region snippetEdit
        [HttpPut]
        public IActionResult Edit([FromBody] TodoItem item)
        {
            try
            {
                if (item == null || !ModelState.IsValid)
                {
                    return BadRequest(ErrorCode.TodoItemNameAndNotesRequired.ToString());
                }
                var existingItem = _todoRepository.Find(item.ID);
                if (existingItem == null)
                {
                    return NotFound(ErrorCode.RecordNotFound.ToString());
                }
                _todoRepository.Update(item);
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotUpdateItem.ToString());
            }
            return NoContent();
        }
        #endregion
        
        #region snippetDelete
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                var item = _todoRepository.Find(id);
                if (item == null)
                {
                    return NotFound(ErrorCode.RecordNotFound.ToString());
                }
                _todoRepository.Delete(id);
            }
            catch (Exception)
            {
                return BadRequest(ErrorCode.CouldNotDeleteItem.ToString());
            }
            return NoContent();
        }
        #endregion
    }
}
