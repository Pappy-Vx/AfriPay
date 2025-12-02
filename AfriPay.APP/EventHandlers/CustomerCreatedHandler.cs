using AfriPay.CORE.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AfriPay.APP.EventHandlers;

public class CustomerCreatedHandler : INotificationHandler<CustomerCreatedEvent>
{
    private readonly ILogger<CustomerCreatedHandler> _logger;

    public CustomerCreatedHandler(ILogger<CustomerCreatedHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(CustomerCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Customer created: {CustomerId}, Name: {FirstName} {LastName}, Email: {Email}",
            notification.CustomerId,
            notification.FirstName,
            notification.LastName,
            notification.Email);

        // Add any additional logic here (e.g., send welcome email, notify external systems)

        return Task.CompletedTask; // <-- This was missing
    }
}