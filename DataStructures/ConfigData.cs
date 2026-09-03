namespace AmmoStatsInNames.DataStructures;

public class ConfigData
{
    public string StatsToAdd { get; set; } = "({PenetrationPower}/{Damage})";
    public string StatsToAddBuckshot { get; set; } = "({PenetrationPower}/{Damage}x{ProjectileCount})";
    public string StatsToAddGrenadeRound { get; set; } = "({PenetrationPower}/{Damage}/{FuzeArmTimeSec}s)";
    public bool StatsBeforeName { get; set; } = true;
    public int PaddingLength { get; set; } = 2;
    public bool Debug { get; set; } = false;
}
