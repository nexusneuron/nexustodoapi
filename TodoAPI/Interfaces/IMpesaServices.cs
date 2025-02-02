﻿using RestSharp;

namespace TodoAPI.Interfaces
{
    public interface IMpesaServices
    {
        void oauth();
        Task<RestResponse> oauth2();

        Task<RestResponse> stkpush();

        Task<RestResponse> CtoBRegisterURL(string accesstoken);

        Task<RestResponse> c2bsimulate();
    }
}
