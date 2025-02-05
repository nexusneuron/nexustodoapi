namespace TodoAPI.Models
{
    public class TempSTKData
    {
        [Key]
        public string? merchantID { get; set; }
        public int? businessShortcode { get; set; }
        public int? amount { get; set; }
        public long? partyA {  get; set; }        
        public string? accNO { get; set; }
        public string? TransTime { get; set; }
        public int? PartyB { get; set; }
        public long? PhoneNumber { get; set; }
        public string? CallBackURL { get; set; }
        //public string? AccountReference { get; set; }
        public string? TransactionDesc { get; set; }
        //public string? passkey { get; set; }
    }
}
