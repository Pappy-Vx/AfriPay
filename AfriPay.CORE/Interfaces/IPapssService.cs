using AfriPay.CORE.Common;
using AfriPay.CORE.ValueObjects;

namespace AfriPay.CORE.Interfaces;

/// <summary>
/// Abstraction over the PAPSS (Pan-African Payment and Settlement System) network.
/// In production this would call the real PAPSS APIs; here we provide a mock implementation.
/// </summary>
public interface IPapssService
{
    /// <summary>
    /// Perform PAPSS settlement for a potential cross-border transfer.
    ///
    /// If source and destination currencies are the same, implementations SHOULD short-circuit
    /// and return a successful result with <see cref="PapssSettlementResult.RequiresPapss"/> = false.
    /// </summary>
    Task<Result<PapssSettlementResult>> SettleAsync(
        PapssSettlementRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request payload for PAPSS settlement.
/// This is intentionally lean and currency-focused so it can be used both in mocks and real integrations.
/// </summary>
public sealed class PapssSettlementRequest
{
    /// <summary>
    /// Logical transfer identifier used for tracing (AfriPay TransferId).
    /// </summary>
    public Guid TransferId { get; init; }

    /// <summary>
    /// Currency of the source leg (e.g. "NGN").
    /// </summary>
    public string SourceCurrency { get; init; } = string.Empty;

    /// <summary>
    /// Currency of the destination leg (e.g. "GHS", "KES").
    /// </summary>
    public string DestinationCurrency { get; init; } = string.Empty;

    /// <summary>
    /// Amount in source currency that should be settled via PAPSS.
    /// </summary>
    public decimal Amount { get; init; }
}

/// <summary>
/// Result of a PAPSS settlement attempt.
/// </summary>
public sealed class PapssSettlementResult
{
    /// <summary>
    /// Indicates whether PAPSS was required for this transfer (cross-currency / cross-country).
    /// </summary>
    public bool RequiresPapss { get; init; }

    /// <summary>
    /// Indicates whether the settlement was successful when PAPSS was required.
    /// For <see cref="RequiresPapss"/> = false this will always be true.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Currency of the source leg used in the computation.
    /// </summary>
    public string SourceCurrency { get; init; } = string.Empty;

    /// <summary>
    /// Currency of the destination leg used in the computation.
    /// </summary>
    public string DestinationCurrency { get; init; } = string.Empty;

    /// <summary>
    /// Amount debited from the source account (in <see cref="SourceCurrency"/>).
    /// </summary>
    public decimal SourceAmount { get; init; }

    /// <summary>
    /// Amount that should be credited to the destination account (in <see cref="DestinationCurrency"/>).
    /// </summary>
    public decimal DestinationAmount { get; init; }

    /// <summary>
    /// FX rate applied: DestinationAmount = SourceAmount * FxRate.
    /// For same-currency flows this is 1.0.
    /// </summary>
    public decimal FxRate { get; init; }

    /// <summary>
    /// Optional failure reason when <see cref="IsSuccess"/> is false.
    /// </summary>
    public string? FailureReason { get; init; }

    public static PapssSettlementResult Skipped(string currency, decimal amount) => new()
    {
        RequiresPapss = false,
        IsSuccess = true,
        SourceCurrency = currency,
        DestinationCurrency = currency,
        SourceAmount = amount,
        DestinationAmount = amount,
        FxRate = 1m,
        FailureReason = null
    };

    public static PapssSettlementResult Success(
        string sourceCurrency,
        string destinationCurrency,
        decimal sourceAmount,
        decimal destinationAmount,
        decimal fxRate) => new()
    {
        RequiresPapss = true,
        IsSuccess = true,
        SourceCurrency = sourceCurrency,
        DestinationCurrency = destinationCurrency,
        SourceAmount = sourceAmount,
        DestinationAmount = destinationAmount,
        FxRate = fxRate,
        FailureReason = null
    };

    public static PapssSettlementResult Failed(
        string sourceCurrency,
        string destinationCurrency,
        decimal sourceAmount,
        string failureReason) => new()
    {
        RequiresPapss = true,
        IsSuccess = false,
        SourceCurrency = sourceCurrency,
        DestinationCurrency = destinationCurrency,
        SourceAmount = sourceAmount,
        DestinationAmount = 0m,
        FxRate = 0m,
        FailureReason = failureReason
    };
}
