namespace SystemService;

public sealed class SystemSettings
{
    public const string SectionName = "System";

    public string ServiceName { get; init; } = "system";

    public string Realm { get; init; } = "n2-system";
}
