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

        public async Task ConfirmPayment(string encodedphoneMerchant)
        {
            Console.WriteLine("Reading from messaging QUEUE after saving to DB");

            await Task.Delay(60 * 1000);

            await _rabbitMQConsumer.ConsumeMessageAsync(encodedphoneMerchant);


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

                //_mpesaservice.oauth();

                //var response = await _mpesaservice.oauth2();

                //if (response.ErrorException != null)
                //{
                //    Console.WriteLine(response.ErrorException.Message);
                //    return BadRequest(ErrorCode.CouldNotCreateItem.ToString());
                //}

                //Console.WriteLine(response.Content);


                ////CtoBRegisterURL
                var ctobresponse = await _mpesaservice.CtoBRegisterURL();
                Console.WriteLine(ctobresponse.Content);


                ////STK Push
                var response = await _mpesaservice.stkpush();


                //Stream reader RAW FROM HTTP REQUEST
                //using var reader = new StreamReader(HttpContext.Request.Body);
                // You now have the body string raw
                //var body = await reader.ReadToEndAsync();

                //Get merchant ID
                Root requestResponse = JsonConvert.DeserializeObject<Root>(response.Content);
                String merchantID = requestResponse.MerchantRequestID;

                //encode phone + amount
                int amount = 1;
                string phone = "254717904391";
                string accNO = "CompanyXLTD";


                byte[] _amtAccNo = Encoding.UTF8.GetBytes(amount + accNO);
                String _encodedamtAccNo = System.Convert.ToBase64String(_amtAccNo);


                //encoded phone merchantID
                //byte[] _phoneMerchant = Encoding.UTF8.GetBytes(phone + merchantID);
                //String _encodedphoneMerchant = System.Convert.ToBase64String(_phoneMerchant);

                RabbitMQQueues queueTitle = new RabbitMQQueues();
                //queueTitle.QueueTitle = response.Content.Count().ToString();
                queueTitle.QueueTitle = _encodedamtAccNo;

                Console.WriteLine(queueTitle.QueueTitle);

                //await ConfirmPayment(_encodedphoneMerchant);

                //send the inserted product data to the queue and consumer will listening this data from queue
                //await _rabbitMQPublisher.SendStkResponseMessage(response.Content);


                //Console.WriteLine("PUBLISHING TO QUEUE");
                //post to queue and read from queue
                //await _rabbitMQPublisher.PublishMessageAsync(response.Content, queueTitle.QueueTitle);

                Console.WriteLine("writing to db first");


                //Console.WriteLine(response.Content);

                _context.TodoItems.Add(item);
                await _context.SaveChangesAsync();
                //_context.SaveChanges();


                Console.WriteLine("Calling ConfirmPayment method");

                try
                {
                    ConfirmPayment(_encodedamtAccNo);
                    //await _rabbitMQConsumer.ConsumeMessageAsync(queueTitle.QueueTitle);
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
