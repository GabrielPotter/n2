namespace Gateway;

public sealed class GatewaySettings
{
    public const string SectionName = "Gateway";

    public string PublicBaseUrl { get; init; } = "http://localhost:5100";
}
