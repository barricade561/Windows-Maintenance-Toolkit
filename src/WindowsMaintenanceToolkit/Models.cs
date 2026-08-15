namespace WindowsMaintenanceToolkit;

public enum LogSeverity { Info, Success, Warning, Error }
public enum RiskLevel { Safe, Caution, RestartRequired }

public sealed record ToolDefinition(
    string Id,
    string Category,
    RiskLevel Risk,
    Func<MainForm, Task> Execute,
    bool LongRunning = false);

public sealed record CommandResult(int ExitCode, string Output, string Error)
{
    public bool Success => ExitCode == 0;
}

public sealed record HealthCheckResult(string Name, bool Passed, string Detail);

public sealed record SystemSnapshot(IReadOnlyDictionary<string, string> Values);
