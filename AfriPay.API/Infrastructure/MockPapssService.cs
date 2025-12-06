using AfriPay.CORE.Common;
using AfriPay.CORE.Interfaces;

namespace AfriPay.API.Infrastructure;

/// <summary>
/// Mock implementation of <see cref="IPapssService"/>.
///
/// This simulates PAPSS behaviour with hard-coded FX rates between a few African currencies.
/// Rules:
/// - Same currency: PAPSS is skipped entirely.
/// - Different currencies: PAPSS is required and we apply a deterministic conversion.
///
/// NOTE: This is for development only. Do NOT use these rates in production.
/// </summary>
public sealed class MockPapssService : IPapssService
{
    private readonly ILogger<MockPapssService> _logger;

    // Simple FX matrix. Keys are "SOURCE-DEST" currency pairs.
    // Example from requirements: 1 NGN = 300 GHS.
    private static readonly IReadOnlyDictionary<string, decimal> FxRates = new Dictionary<string, decimal>(
        StringComparer.OrdinalIgnoreCase)
    {
        // Nigeria ↔ Ghana
        ["NGN-GHS"] = 0.0076m,          // 1 NGN -> 0.0076 GHS (current rate)
        ["GHS-NGN"] = 127.14m,          // 1 GHS -> 127.14 NGN

        // Nigeria ↔ Kenya
        ["NGN-KES"] = 0.0896m,          // 1 NGN -> 0.0896 KES
        ["KES-NGN"] = 11.19m,           // 1 KES -> 11.19 NGN

        // Ghana ↔ Kenya
        ["GHS-KES"] = 10.72m,           // 1 GHS -> 10.72 KES
        ["KES-GHS"] = 0.093m            // 1 KES -> 0.093 GHS
    };

    public MockPapssService(ILogger<MockPapssService> logger)
    {
        _logger = logger;
    }

    public Task<Result<PapssSettlementResult>> SettleAsync(
        PapssSettlementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.SourceCurrency))
            return Task.FromResult(Result.Failure<PapssSettlementResult>("Source currency is required"));

        if (string.IsNullOrWhiteSpace(request.DestinationCurrency))
            return Task.FromResult(Result.Failure<PapssSettlementResult>("Destination currency is required"));

        if (request.Amount <= 0)
            return Task.FromResult(Result.Failure<PapssSettlementResult>("Amount must be greater than zero"));

        var source = request.SourceCurrency.Trim().ToUpperInvariant();
        var dest = request.DestinationCurrency.Trim().ToUpperInvariant();

        // Same-currency: PAPSS is effectively a no-op
        if (string.Equals(source, dest, StringComparison.OrdinalIgnoreCase))
        {
            var skipped = PapssSettlementResult.Skipped(source, request.Amount);

            _logger.LogInformation(
                "PAPSS skipped for Transfer {TransferId}. Same currency {Currency}, Amount {Amount}",
                request.TransferId,
                source,
                request.Amount);

            return Task.FromResult(Result.Success(skipped));
        }

        var fxKey = $"{source}-{dest}";
        if (!FxRates.TryGetValue(fxKey, out var fxRate) || fxRate <= 0)
        {
            var failed = PapssSettlementResult.Failed(
                source,
                dest,
                request.Amount,
                $"No PAPSS FX rate configured for {source}->{dest}");

            _logger.LogWarning(
                "PAPSS settlement failed for Transfer {TransferId}: {Reason}",
                request.TransferId,
                failed.FailureReason);

            return Task.FromResult(Result.Success(failed));
        }

        var destinationAmount = Math.Round(request.Amount * fxRate, 2, MidpointRounding.AwayFromZero);
        var result = PapssSettlementResult.Success(
            source,
            dest,
            request.Amount,
            destinationAmount,
            fxRate);

        _logger.LogInformation(
            "PAPSS settlement simulated for Transfer {TransferId}: {SourceAmount} {SourceCurrency} -> {DestinationAmount} {DestinationCurrency} @ FX {FxRate}",
            request.TransferId,
            result.SourceAmount,
            result.SourceCurrency,
            result.DestinationAmount,
            result.DestinationCurrency,
            result.FxRate);

        return Task.FromResult(Result.Success(result));
    }
}
