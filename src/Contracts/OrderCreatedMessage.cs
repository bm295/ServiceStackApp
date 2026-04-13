namespace ServiceStackApp.ServiceModel;

public class OrderCreatedMessage
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
