using SPTarkov.Server.Core.Models.Spt.Mod;

namespace AmmoStatsInNames;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.szonszczyk.ammostatsinnames";
    public string Name { get; init; } = "AmmoStatsInNames";
    public string Author { get; init; } = "Szonszczyk";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.3");
    public List<string>? Incompatibilities { get; init; } = [];
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = [];
    public string? Url { get; init; } = "https://github.com/Szonszczyk/AmmoStatsInNames";
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
}
