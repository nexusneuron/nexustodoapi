namespace TodoAPI.Models
{
    public class NexuspayConfirmation
    {
        public string? TransactionType { get; set; } //"TransactionType": "Pay Bill"

        [Key]
        public string? TransID { get; set; }  // "TransID":"RKTQDM7W6S",
        public DateTime? TransTime { get; set; }   // "TransTime":"20191122063845",
        public string? TransAmount { get; set; }   //   int "TransAmount":"10"
        public string? BusinessShortCode { get; set; }  //   "BusinessShortCode": "600638",
        public string? BillRefNumber { get; set; }   //   "BillRefNumber":"invoice008",
        //public string? InvoiceNumber { get; set; }    //   "InvoiceNumber":"",
        public string? OrgAccountBalance { get; set; }   //   Decimal "OrgAccountBalance":""
        public string? ThirdPartyTransID { get; set; } //    "ThirdPartyTransID": "",
        public string? MSISDN { get; set; }  //   "MSISDN":"25470****149",
        public string? FirstName { get; set; }   //     "FirstName":"John",
        //public string? MiddleName { get; set; }   //   "MiddleName":""
        //public string? LastName { get; set; }    //   "LastName":"Doe"

    }
}
