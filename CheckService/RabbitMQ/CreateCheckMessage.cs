namespace CheckService.RabbitMQ
{
    public class CreateCheckMessage
    {
        public string OrderId { get; set; }
        public List<CheckItem> Items { get; set; }
    }
}
