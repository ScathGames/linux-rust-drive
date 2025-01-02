namespace ProtonDrive.App.Instrumentation.Observability;

internal sealed record AttemptRetryCounters(int FirstAttemptSuccesses, int FirstAttemptFailures, int RetrySuccesses, int RetryFailures)
{
    public bool Any()
    {
        return (FirstAttemptSuccesses + FirstAttemptFailures + RetrySuccesses + RetryFailures) > 0;
    }
}
