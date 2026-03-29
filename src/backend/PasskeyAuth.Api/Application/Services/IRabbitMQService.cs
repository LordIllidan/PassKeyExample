namespace PasskeyAuth.Api.Application.Services;

public interface IRabbitMQService
{
    Task PublishAsync(string routingKey, object message);
    Task SubscribeAsync(string queue, Func<object, Task> handler);
    Task DeclareExchangeAsync(string name, string type);
    Task DeclareQueueAsync(string name, Dictionary<string, object>? arguments = null);
    Task BindQueueAsync(string queue, string exchange, string routingKey);
    Task InitializeAsync();
}
