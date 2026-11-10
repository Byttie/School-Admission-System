namespace AdmissionSystem.Api;

public record HoldSettings(TimeSpan Duration)
{
    public const int DefaultSeconds = 600;

    public static HoldSettings FromConfiguration(IConfiguration config)
    {
        var seconds = config.GetValue<int?>("HOLD_DURATION_SECONDS") ?? DefaultSeconds;
        if (seconds <= 0) seconds = DefaultSeconds;
        return new HoldSettings(TimeSpan.FromSeconds(seconds));
    }
}