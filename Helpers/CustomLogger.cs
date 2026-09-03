using AmmoStatsInNames.DataStructures;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Color = Spectre.Console.Color;

namespace AmmoStatsInNames.Helpers;

/// <summary>
/// Provides consistently formatted, colour-coded mod log messages.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class CustomLogger(ISptLogger<AmmoStatsInNamesMod> logger, ConfigData config)
{
    /// <summary>
    /// Enables or suppresses messages written through <see cref="Debug"/>.
    /// </summary>

    public void Ok(string message, [CallerMemberName] string functionName = "")
        => Log(message, Color.Green, null, functionName);

    public void Info(string message, [CallerMemberName] string functionName = "")
        => Log(message, Color.White, null, functionName);

    public void Warning(string message, [CallerMemberName] string functionName = "")
        => Log(message, Color.Yellow, null, functionName);

    public void Error(string message, [CallerMemberName] string functionName = "")
        => Log(message, Color.Red, Color.White, functionName);

    public void Debug(string message, [CallerMemberName] string functionName = "")
    {
        if (config.Debug)
            Log(message, Color.HotPink, null, functionName);
    }

    public void Important(string message, [CallerMemberName] string functionName = "")
        => Log(message, Color.Yellow, Color.Red, functionName);

    private void Log(string message, Color textColor, Color? backgroundColor, string functionName)
    {
        var namespaceName = new StackTrace()
            .GetFrames()?
            .Select(frame => frame.GetMethod()?.DeclaringType)
            .FirstOrDefault(type => type is not null && type != typeof(CustomLogger))
            ?.Namespace ?? "Unknown";

        logger.LogWithColor($"[{namespaceName}/{functionName}] {message}", textColor, backgroundColor);
    }
}
