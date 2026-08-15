using System.Text.Json;

namespace WindowsMaintenanceToolkit;

public sealed class AppSettings
{
    public string Language { get; set; } = "en";
    public bool MinimizeToTray { get; set; }
    public bool ConfirmRiskyOperations { get; set; } = true;
    public bool AutoRestorePoint { get; set; } = true;
    public int LogRetentionDays { get; set; } = 14;
    public bool Animations { get; set; } = true;

    private static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Barracuda Systems", "Windows Maintenance Toolkit");
    private static string FilePath => Path.Combine(Folder, "settings.json");

    public static AppSettings Load()
    {
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new(); }
        catch { return new(); }
    }

    public void Save()
    {
        Directory.CreateDirectory(Folder);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
