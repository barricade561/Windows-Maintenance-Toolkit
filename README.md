# Windows Maintenance Toolkit

A modern Windows 10/11 maintenance, repair, diagnostics, cleanup, and optimization utility built with C#/.NET and native Windows servicing tools.

## Features

- **SFC Scan** — runs `sfc /scannow`
- **DISM CheckHealth** — checks component-store corruption state
- **DISM ScanHealth** — performs a deeper component-store scan
- **DISM RestoreHealth** — repairs the Windows component store
- **CHKDSK Online Scan** — runs `chkdsk C: /scan`
- **Flush DNS** — clears the local DNS resolver cache
- **Reset Winsock** — resets the Windows Winsock catalog
- **Temporary File Cleanup** — removes deletable files from Windows/user temp folders while skipping locked files
- **System Information** — displays basic local system diagnostics
- **Full System Health Check** — runs DISM, SFC, and CHKDSK in sequence
- Built-in activity log showing the commands and their output
- Administrator manifest for operations that require elevation
- Dark Windows desktop interface

## Download / EXE build

Every push to `main` triggers the GitHub Actions workflow **Build Windows EXE**.

1. Open the repository's **Actions** tab.
2. Open the latest successful **Build Windows EXE** run.
3. Download the `Windows-Maintenance-Toolkit-win-x64` artifact.
4. Extract the archive and run `WindowsMaintenanceToolkit.exe`.

The executable is published as a **self-contained Windows x64 single-file application**, so a separate .NET installation is not required.

## Safety notes

This project intentionally uses standard Windows maintenance utilities instead of aggressive "debloat" behavior. Commands are shown in the interface and their output is logged. Some operations can take a long time. Winsock reset may require a reboot.

`DISM /RestoreHealth` may use Windows Update or another configured repair source to obtain replacement component files.

## Requirements

- Windows 10 or Windows 11 x64
- Administrator approval through UAC

## Development

Project:

```text
src/WindowsMaintenanceToolkit/WindowsMaintenanceToolkit.csproj
```

Build locally with .NET 8 SDK:

```powershell
dotnet publish .\src\WindowsMaintenanceToolkit\WindowsMaintenanceToolkit.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

---

# Türkçe

Windows Maintenance Toolkit; Windows 10/11 sistemlerinde bakım, onarım, tanılama ve temel temizlik işlemlerini grafik arayüz üzerinden çalıştırmak için geliştirilmiş bir masaüstü uygulamasıdır.

## Özellikler

- `sfc /scannow`
- DISM CheckHealth / ScanHealth / RestoreHealth
- `chkdsk C: /scan`
- DNS önbelleğini temizleme
- Winsock sıfırlama
- Geçici dosya temizliği
- Sistem bilgilerini görüntüleme
- SFC + DISM + CHKDSK içeren toplu sistem sağlık kontrolü
- Çalıştırılan komutları ve çıktılarını gösteren aktivite günlüğü
- Yönetici yetkisi isteyen işlemler için UAC desteği
- Koyu ve modern Windows arayüzü

`.exe` dosyası GitHub Actions tarafından otomatik olarak oluşturulur. Repository içindeki **Actions** sekmesinden en son başarılı **Build Windows EXE** çalıştırmasını açıp `Windows-Maintenance-Toolkit-win-x64` artifact'ini indirebilirsiniz.
