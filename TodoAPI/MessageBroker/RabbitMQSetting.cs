namespace TodoAPI.MessageBroker
{
    public class RabbitMQSetting
    {
        public string? HostName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    //RabbitMQ Queue name
    public static class RabbitMQQueues
    {
        public const string OrderValidationQueue = "orderValidationQueue";
        public const string stkcallbackqueue = "stkcallbackqueue";
        public const string ThirdQueue = "thirdQueue";
    }
}
