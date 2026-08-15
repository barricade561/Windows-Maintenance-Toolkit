namespace WindowsMaintenanceToolkit;

internal static class L
{
    public static string Language { get; set; } = "en";

    private static readonly Dictionary<string, (string En, string Tr)> Texts = new()
    {
        ["subtitle"] = ("Windows command center for maintenance and diagnostics", "Windows bakım ve tanılama komuta merkezi"),
        ["ready"] = ("Ready", "Hazır"), ["running"] = ("Running", "Çalışıyor"),
        ["clear"] = ("CLEAR LOG", "GÜNLÜĞÜ TEMİZLE"), ["export"] = ("EXPORT LOG", "GÜNLÜĞÜ DIŞA AKTAR"),
        ["activity"] = ("ACTIVITY LOG", "ETKİNLİK GÜNLÜĞÜ"), ["run"] = ("RUN", "ÇALIŞTIR"),
        ["dashboard"] = ("Dashboard", "Gösterge Paneli"), ["system_repair"] = ("System Repair", "Sistem Onarımı"),
        ["cleanup"] = ("Cleanup", "Temizlik"), ["network"] = ("Network", "Ağ"),
        ["optimization"] = ("Optimization", "Optimizasyon"), ["diagnostics"] = ("Diagnostics", "Tanılama"),
        ["security"] = ("Security", "Güvenlik"), ["system_info"] = ("System Info", "Sistem Bilgisi"),
        ["startup_services"] = ("Startup & Services", "Başlangıç ve Hizmetler"),
        ["windows_update"] = ("Windows Update", "Windows Update"),
        ["recovery"] = ("Restore & Recovery", "Geri Yükleme ve Kurtarma"),
        ["utilities"] = ("Utilities / Scheduler", "Araçlar / Zamanlayıcı"),
        ["logs"] = ("Logs", "Günlükler"), ["settings"] = ("Settings", "Ayarlar"), ["about"] = ("About", "Hakkında"),
        ["confirm"] = ("This operation can change Windows configuration. Continue?", "Bu işlem Windows yapılandırmasını değiştirebilir. Devam edilsin mi?"),
        ["safe"] = ("SAFE", "GÜVENLİ"), ["caution"] = ("CAUTION", "DİKKAT"),
        ["restart"] = ("RESTART REQUIRED", "YENİDEN BAŞLATMA GEREKLİ"),
        ["loading"] = ("Collecting live system information…", "Canlı sistem bilgileri toplanıyor…"),
        ["no_percent"] = ("Health is based on real checks; no synthetic score is used.", "Sağlık durumu gerçek kontrollere dayanır; yapay puan kullanılmaz."),
    };

    public static string T(string key) => Texts.TryGetValue(key, out var pair) ? (Language == "tr" ? pair.Tr : pair.En) : key;
}
