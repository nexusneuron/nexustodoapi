using RestSharp;

namespace TodoAPI.Interfaces
{
    public interface IMpesaServices
    {
        void oauth();
        Task<RestResponse> oauth2();

        //Task<RestResponse> stkpush();
        Task<RestResponse> stkpush(int businessShortcode, int amount, long partyA, string accNO, string TransTime, int PartyB, long PhoneNumber, string CallBackURL, string TransactionDesc, string passkey);


        //Task<RestResponse> CtoBRegisterURL(string accesstoken);
        Task<RestResponse> CtoBRegisterURL();

        Task<RestResponse> c2bsimulate();
    }
}
