using MediatR;

namespace AfriPay.APP.EventHandlers;

/// <summary>
/// Application-level notification that PAPSS settlement has completed successfully for a transfer.
/// This is intentionally decoupled from the domain model so PAPSS can evolve without forcing migrations.
/// </summary>
public sealed record PapssSettlementCompletedNotification(
    Guid TransferId,
    string SourceCurrency,
    string DestinationCurrency,
    decimal SourceAmount,
    decimal DestinationAmount,
    decimal FxRate
) : INotification;
