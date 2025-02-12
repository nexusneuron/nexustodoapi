﻿namespace TodoAPI.Models
{
    public class StkCallback
    {
        //public string? ID { get; set; }
        public string? MerchantRequestID { get; set; } //"MerchantRequestID": "29115-34620561-1",
        public string? CheckoutRequestID { get; set; }    //            "CheckoutRequestID": "ws_CO_191220191020363925",
        public int? Amount { get; set; }    //            "Amount": "1",
        public long? PhoneNumber { get; set; }    //    LONG DATATYPE        "PhoneNumber":"254708374149",
        public string? AccountReference { get; set; }     //            "AccountReference":"AccountNumbee" 
        public string? TransactionDesc { get; set; }    //            "TransactionDesc":"Test"

        [Key]
        public string? MpesaReceiptNumber { get; set; }   //            "MpesaReceiptNumber",
        public string? TransactionDate { get; set; }   //               "TransactionDate",
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
    }
}



        //"Amount"

        //"MpesaReceiptNumber"

        //"TransactionDate"

        //"PhoneNumber