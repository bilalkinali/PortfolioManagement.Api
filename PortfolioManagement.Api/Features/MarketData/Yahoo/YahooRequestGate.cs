using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace PortfolioManagement.Api.Features.MarketData.Yahoo;

public sealed class YahooRequestGate
{
    private static readonly TimeSpan MinimumDelay = TimeSpan.FromSeconds(1);

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ResiliencePipeline _pipeline;
    private DateTimeOffset? _lastRequestUtc;

    public YahooRequestGate()
    {
        _pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(10))
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(250),
                BackoffType = DelayBackoffType.Exponential
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 4,
                BreakDuration = TimeSpan.FromMinutes(1)
            })
            .Build();
    }

    internal async Task<T?> ExecuteAsync<T>(
        Func<CancellationToken, Task<T?>> operation,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_lastRequestUtc is not null)
            {
                var elapsed = DateTimeOffset.UtcNow - _lastRequestUtc.Value;

                if (elapsed < MinimumDelay)
                {
                    await Task.Delay(MinimumDelay - elapsed, cancellationToken);
                }
            }

            return await _pipeline.ExecuteAsync(
                async token => await operation(token),
                cancellationToken);
        }
        finally
        {
            _lastRequestUtc = DateTimeOffset.UtcNow;
            _semaphore.Release();
        }
    }
}
