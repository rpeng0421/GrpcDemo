namespace GrpcDemo.Client.Model;

public interface IDomainEvent
{
    public string EventId { get; set; }
    public string Type { get; set; }
    public string Data { get; set; }
    public long Timestamp { get; set; }
}

public class DomainEvent : IDomainEvent
{
    public string EventId { get; set; }
    public string Type { get; set; }
    public string Data { get; set; }
    public long Timestamp { get; set; }
}