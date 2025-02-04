using System.Net;
using System.Text;
using System;
using System.IO;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
using TodoAPI.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Controllers;
using System.Security.Principal;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace TodoAPI.Services
{
    public class mpesaservices : IMpesaServices
    {
        //private readonly HttpClient RestClient;
        //HttpClient RestClient = new HttpClient();

        //public mpesaservices(HttpClient restClient)
        //{
        //    
        //}

        public void oauth()
        {
            string a = "https://sandbox.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials";
            string baseUrl = a;

            String app_key = "u11qBPpOexjQE4TXGCG5t0rIiFj0qJpjyW3FfP0KnaOS8N7x";
            String app_secret = "U4uUcGT0BkurOSAZnMrdQnCM7HUchS9AG2hOOzCLATZ2su0WsX5AL8Rsd2aHNoci";

            byte[] auth = Encoding.UTF8.GetBytes(app_key + ":" + app_secret);
            String encoded = System.Convert.ToBase64String(auth);

            //HttpClient RestClient = new HttpClient();
            var client = new RestClient(baseUrl);
            var request = new  RestRequest();
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl);

            //request.Headers.Add("Authorization", "Basic " + encoded);
            //request.ContentType = "application/json";
            //request.Headers.Add("cache-control", "no-cache");
            request.Method = RestSharp.Method.Get;

            request.AddHeader("Authorization", "Basic " + encoded);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            //RestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);

            try
            {
                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Get the stream associated with the response.
                //Stream receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                //StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                //Console.WriteLine(readStream.ReadToEnd());
                //response.Close();
                //readStream.Close();
                RestResponse response = client.Execute(request);
                if (response.ErrorException != null)
                {
                    Console.WriteLine(response.ErrorException.Message);
                    return;
                }
                Console.WriteLine(response.Content);
            }
            catch (WebException ex)
            {
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                Console.WriteLine(resp);

            }
        }

        public async Task<RestResponse> oauth2()
        {
            //string baseUrl = "https://sandbox.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials";
            string baseUrl = "https://api.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials";

            //String app_key = "u11qBPpOexjQE4TXGCG5t0rIiFj0qJpjyW3FfP0KnaOS8N7x";
            //String app_secret = "U4uUcGT0BkurOSAZnMrdQnCM7HUchS9AG2hOOzCLATZ2su0WsX5AL8Rsd2aHNoci";

            String app_key = "myS6LRlWIU5a932YTdTpdc2mWorS8B76LPpGNqPw07HptRGo";
            String app_secret = "ACKsHFhGDfGkCKXC2rzANX0YGzJN9H1Q33hJwIrgG0iAUPfRkN25M3EEd8UrFqKo";

            byte[] auth = Encoding.UTF8.GetBytes(app_key + ":" + app_secret);
            String encoded = System.Convert.ToBase64String(auth);

            var client = new RestClient(baseUrl);
            var request = new RestRequest();

            //request.AddHeader("ContentType", "application/json");
            //request.AddHeader("cache-control", "no-cache");

            request.Method = RestSharp.Method.Get;

            request.AddHeader("Authorization", "Basic " + encoded);
            request.AddParameter("text/plain", "", ParameterType.RequestBody);
            

            var response = await client.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                Console.WriteLine(response.ErrorException.Message);
                return response;
            }

            Console.WriteLine(response.Content);
            return response;

        }


        public class TypeHere
        {
            public string access_token { get; set; }
        }

        /// <summary>
        /// stk request class
        /// </summary>
        public class stkrequest
        {
            //public string access_token { get; set; }
            public string Password { get; set; }
            public int BusinessShortCode { get; set; }
            public string Timestamp { get; set; }
            public string TransactionType { get; set; }
            public int Amount { get; set; }
            public long PartyA { get; set; }
            public int PartyB { get; set; }
            public long PhoneNumber { get; set; }
            public string CallBackURL { get; set; }
            public string AccountReference { get; set; }
            public string TransactionDesc { get; set; }

        }

        public async Task<RestResponse> stkpush()
        {
            //string baseUrl = "https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest";
            string baseUrl = "https://api.safaricom.co.ke/mpesa/stkpush/v1/processrequest";


            var client = new RestClient(baseUrl);
            var request = new RestRequest();
            request.Method = RestSharp.Method.Post;

            var getaccesstoken = await oauth2();

            if (getaccesstoken.ErrorException != null)
            {
                Console.WriteLine(getaccesstoken.ErrorException.Message);
                return getaccesstoken;
            }

            Console.WriteLine(getaccesstoken.Content);


            TypeHere typeHere = JsonConvert.DeserializeObject<TypeHere>(getaccesstoken.Content);
            string _accesstoken = typeHere.access_token;

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + _accesstoken);

            Console.WriteLine(_accesstoken);

            DateTime d = DateTime.Now;
            string dateString = d.ToString("yyyyMMddHHmmss");

            //string passkey = "bfb279f9aa9bdbcf158e97dd71a467cd2e0c893059b10f78e6b72ada1ed2c919";
            string passkey = "6b4ef1cdca85cb784b049776727e927d73dcdcd717444e51a53e0e7579e5dad6";

            //int shortcode = 174379;
            int shortcode = 5142254;

            byte[] _password = Encoding.UTF8.GetBytes(shortcode + passkey + dateString);
            String _encodedpassword = System.Convert.ToBase64String(_password);

            ////////////////////////////////
            stkrequest stk = new stkrequest(){
                Password = _encodedpassword,
                BusinessShortCode = shortcode,
                Timestamp = dateString,
                TransactionType = "CustomerPayBillOnline",
                Amount = 1,
                PartyA = 254717904391,
                PartyB = shortcode,
                PhoneNumber = 254717904391,
                //CallBackURL = "https://buzzard-hip-donkey.ngrok-free.app/api/stkcallbacks",
                //CallBackURL = "https://testsite.nexusneuron.com/api/stkcallbacks",
                CallBackURL = "https://nexuspay.nexusneuron.com/api/stkcallbacks",

                AccountReference = "NexuspayIni",
                TransactionDesc = "NexuspayPro"
            };

            string jsonstk = JsonConvert.SerializeObject(stk, Formatting.Indented);

            Console.WriteLine(jsonstk);


            request.AddParameter("application/json", jsonstk ,  ParameterType.RequestBody);
            ////////////////////////////////

            var response = await client.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                Console.WriteLine(response.ErrorException.Message);
				Console.WriteLine(response.ErrorMessage);
				Console.WriteLine(response.StatusCode);
				Console.WriteLine(response.Content);
                return response;
            }

            Console.WriteLine(response.Content);
            return response;

        }

        /// <summary>
        /// C2B URL REGISTRATION class
        /// </summary>
        public class ctoburl
        {
            public int ShortCode { get; set; }
            public string ResponseType { get; set; }
            public string ConfirmationURL { get; set; }
            public string ValidationURL { get; set; }

        }

        //public async Task<RestResponse> CtoBRegisterURL(string accesstoken)
        public async Task<RestResponse> CtoBRegisterURL()
        {
            //string baseUrl = "https://sandbox.safaricom.co.ke/mpesa/c2b/v1/registerurl https://api.safaricom.co.ke/mpesa/c2b/v1/registerurl";
            string baseUrl = "https://api.safaricom.co.ke/mpesa/c2b/v2/registerurl";


            var client = new RestClient(baseUrl);
            var request = new RestRequest();
            request.Method = RestSharp.Method.Post;

            var getaccesstoken = await oauth2();

            if (getaccesstoken.ErrorException != null)
            {
                Console.WriteLine(getaccesstoken.ErrorException.Message);
                return getaccesstoken;
            }

            Console.WriteLine(getaccesstoken.Content);


            TypeHere typeHere = JsonConvert.DeserializeObject<TypeHere>(getaccesstoken.Content);
            var _accesstoken = typeHere.access_token;

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + _accesstoken);

            ctoburl ctob = new ctoburl()
            {
                //ShortCode = 600989,
                ShortCode = 5142254,

                ResponseType = "Completed",
                //ConfirmationURL = "https://buzzard-hip-donkey.ngrok-free.app/api/ctobconfirmations",
                //ValidationURL = "https://buzzard-hip-donkey.ngrok-free.app",
                //ConfirmationURL = "https://testsite.nexusneuron.com/api/ctobconfirmations",
                //ValidationURL = "https://testsite.nexusneuron.com",

                ConfirmationURL = "https://nexuspay.nexusneuron.com/api/ctobconfirmations",
                ValidationURL = "https://nexuspay.nexusneuron.com",
            };

            string jsonctob = JsonConvert.SerializeObject(ctob, Formatting.Indented);

            request.AddParameter("application/json", jsonctob, ParameterType.RequestBody);
            ////////////////////////////////

            var response = await client.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                Console.WriteLine(response.ErrorException.Message);
                return response;
            }

            Console.WriteLine(response.Content);
            return response;

        }

        public class c2bsim
        {
            public int ShortCode { get; set; }
            public string CommandID { get; set; }
            public int Amount { get; set; }
            public long Msisdn { get; set; }
            public string BillRefNumber { get; set; }
        }

        public async Task<RestResponse> c2bsimulate()
        {
            string baseUrl = "https://sandbox.safaricom.co.ke/mpesa/c2b/v1/simulate";

            var client = new RestClient(baseUrl);
            var request = new RestRequest();
            request.Method = RestSharp.Method.Post;

            var getaccesstoken = await oauth2();

            if (getaccesstoken.ErrorException != null)
            {
                Console.WriteLine(getaccesstoken.ErrorException.Message);
                return getaccesstoken;
            }

            Console.WriteLine(getaccesstoken.Content);


            TypeHere typeHere = JsonConvert.DeserializeObject<TypeHere>(getaccesstoken.Content);
            var _accesstoken = typeHere.access_token;

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + _accesstoken);

            Console.WriteLine(_accesstoken);

            //DateTime d = DateTime.Now;
            //string dateString = d.ToString("yyyyMMddHHmmss");

            //string passkey = "bfb279f9aa9bdbcf158e97dd71a467cd2e0c893059b10f78e6b72ada1ed2c919";
            int shortcode = 600992;
            //byte[] _password = Encoding.UTF8.GetBytes(shortcode + passkey + dateString);
            //String _encodedpassword = System.Convert.ToBase64String(_password);

            ////////////////////////////////
            c2bsim c2b = new c2bsim()
            {
                CommandID = "CustomerPayBillOnline",
                ShortCode = shortcode,
                Amount = 1,
                Msisdn = 254708374149,
                BillRefNumber = "CompanyXLTD",
            };

            string jsonstk = JsonConvert.SerializeObject(c2b, Formatting.Indented);

            Console.WriteLine(jsonstk);

            request.AddParameter("application/json", jsonstk, ParameterType.RequestBody);
            ////////////////////////////////


            //Registered URLs
            //await CtoBRegisterURL(_accesstoken);
            await CtoBRegisterURL();



            //Simulate c2b confirmation
            var response = await client.ExecuteAsync(request);
            if (response.ErrorException != null)
            {
                Console.WriteLine(response.ErrorException.Message);
                return response;
            }

            Console.WriteLine(response.Content);
            return response;

        }

    }
}




