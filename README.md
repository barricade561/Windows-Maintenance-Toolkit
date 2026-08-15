# Windows Maintenance Toolkit

**Barracuda Systems** presents a bilingual Windows 10/11 maintenance, repair, security and diagnostics command center. Version 2 uses an obsidian-black interface with crimson neon accents, transparent command execution, risk labels and evidence-backed health reporting.

## Highlights

- English / Turkish live language selector
- Comprehensive dashboard: CPU, GPU, RAM, disks, network, Windows build, uptime, motherboard, BIOS/UEFI, Secure Boot, TPM, virtualization, computer/user identity, Defender and firewall state
- Category navigation: Dashboard, System Repair, Cleanup, Network, Optimization, Diagnostics, Security, System Info, Startup & Services, Windows Update, Restore & Recovery, Utilities / Scheduler, Logs, Settings and About
- Preserved tools: SFC, DISM CheckHealth / ScanHealth / RestoreHealth, CHKDSK, Flush DNS, Reset Winsock, Temp Cleanup, System Info and Full Health Check
- Additional safe tools: restore points, startup inventory, controlled Services console, Windows Update cache repair, Disk Cleanup, ping, traceroute, DNS/gateway tests, power-plan selection and Windows system-console shortcuts
- `SAFE`, `CAUTION` and `RESTART REQUIRED` labels, confirmation prompts and optional automatic restore points
- Single-operation guard, running state and progress indication
- Persistent activity logs with timestamps, severity, exit codes, export, clear and configurable retention
- Full Health Check derives `HEALTHY` / `ATTENTION REQUIRED` from actual DISM, SFC and CHKDSK results. It never invents a health percentage.
- No random debloat and no automatic disabling of critical services

## Download and build

GitHub Actions builds and verifies a self-contained Windows x64 single-file executable on pushes and pull requests targeting `main`. Download the `Windows-Maintenance-Toolkit-win-x64` artifact from a successful **Build Windows EXE** run.

```powershell
dotnet publish .\src\WindowsMaintenanceToolkit\WindowsMaintenanceToolkit.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

Requirements: Windows 10/11 x64 and UAC administrator approval. DISM RestoreHealth may use Windows Update. Network reset and Windows Update repair can require a restart.

---

# Türkçe

**Barracuda Systems** tarafından geliştirilen Windows Maintenance Toolkit; Windows 10/11 için çift dilli bakım, onarım, güvenlik ve tanılama komuta merkezidir. Obsidian siyah arayüzü crimson neon vurgularla birleştirir; çalıştırılan komutları açıkça gösterir ve riskli işlemleri onaya bağlar.

## Öne çıkanlar

- Sağ üstte anlık İngilizce / Türkçe dil seçimi
- CPU, GPU, RAM, diskler, ağ, Windows sürümü/build, uptime, anakart, BIOS/UEFI, Secure Boot, TPM, sanallaştırma, bilgisayar/kullanıcı, Defender ve güvenlik duvarı bilgilerini gösteren kapsamlı Dashboard
- Sistem Onarımı, Temizlik, Ağ, Optimizasyon, Tanılama, Güvenlik, Sistem Bilgisi, Başlangıç ve Hizmetler, Windows Update, Geri Yükleme ve Kurtarma, Araçlar / Zamanlayıcı, Günlükler, Ayarlar ve Hakkında bölümleri
- SFC, DISM, CHKDSK, ağ testleri, restore point, Update onarımı, temizlik ve Windows yönetim kısayolları
- `GÜVENLİ`, `DİKKAT` ve `YENİDEN BAŞLATMA GEREKLİ` etiketleri; onay pencereleri ve isteğe bağlı otomatik geri yükleme noktası
- Zaman, seviye ve exit code içeren kalıcı günlükler; temizleme, dışa aktarma ve saklama süresi ayarı
- Full Health Check yalnızca gerçek DISM, SFC ve CHKDSK sonuçlarından durum üretir; uydurma sağlık yüzdesi kullanmaz.
- Rastgele debloat uygulanmaz ve kritik servisler otomatik kapatılmaz.

GitHub Actions, `main` dalına yönelik push ve pull requestlerde self-contained Windows x64 tek dosya EXE üretir ve doğrular. Başarılı **Build Windows EXE** çalışmasından `Windows-Maintenance-Toolkit-win-x64` artifact’i indirilebilir.

Gereksinimler: Windows 10/11 x64 ve UAC yönetici onayı.
