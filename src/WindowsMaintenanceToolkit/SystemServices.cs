using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Win32;

namespace WindowsMaintenanceToolkit;

internal static class SystemServices
{
    public static async Task<CommandResult> RunAsync(string file, string args, Action<string>? onLine = null, CancellationToken token = default)
    {
        var stdout = new StringBuilder(); var stderr = new StringBuilder();
        var psi = new ProcessStartInfo(file, args)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
        p.OutputDataReceived += (_, e) => { if (e.Data is not null) { stdout.AppendLine(e.Data); onLine?.Invoke(e.Data); } };
        p.ErrorDataReceived += (_, e) => { if (e.Data is not null) { stderr.AppendLine(e.Data); onLine?.Invoke("[stderr] " + e.Data); } };
        if (!p.Start()) throw new InvalidOperationException($"Could not start {file}.");
        p.BeginOutputReadLine(); p.BeginErrorReadLine();
        await p.WaitForExitAsync(token);
        return new(p.ExitCode, stdout.ToString(), stderr.ToString());
    }

    public static async Task<SystemSnapshot> SnapshotAsync()
    {
        var v = new Dictionary<string, string>();
        v["Computer"] = Environment.MachineName; v["User"] = $"{Environment.UserDomainName}\\{Environment.UserName}";
        v["CPU"] = await Ps("(Get-CimInstance Win32_Processor | Select-Object -First 1 -ExpandProperty Name)");
        v["GPU"] = await Ps("((Get-CimInstance Win32_VideoController | Select-Object -ExpandProperty Name) -join ', ')");
        v["RAM"] = await Ps("$c=Get-CimInstance Win32_ComputerSystem; '{0:N1} GB' -f ($c.TotalPhysicalMemory/1GB)");
        v["Windows"] = GetWindowsDisplayName();
        v["Uptime"] = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(@"d\.hh\:mm\:ss", CultureInfo.InvariantCulture);
        v["Motherboard"] = await Ps("$b=Get-CimInstance Win32_BaseBoard; \"$($b.Manufacturer) $($b.Product)\"");
        v["BIOS / UEFI"] = await Ps("$b=Get-CimInstance Win32_BIOS | Select-Object -First 1; \"$($b.Manufacturer) $($b.SMBIOSBIOSVersion)\"");
        v["Secure Boot"] = await Ps("try { if(Confirm-SecureBootUEFI){'Enabled'}else{'Disabled'} } catch {'Unsupported / Legacy BIOS'}");
        v["TPM"] = await Ps("$t=Get-Tpm; if($t.TpmPresent){if($t.TpmReady){'Present · Ready'}else{'Present · Not ready'}}else{'Not present'}");
        v["Virtualization"] = await Ps("$c=Get-CimInstance Win32_ComputerSystem; if($c.HypervisorPresent){'Hypervisor active'}else{'Hypervisor not active'}");
        v["Storage"] = string.Join(" · ", DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed).Select(d => $"{d.Name} {Format(d.AvailableFreeSpace)} free / {Format(d.TotalSize)}"));
        v["Network"] = NetworkInterface.GetAllNetworkInterfaces().Any(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback) ? "Connected" : "Disconnected";
        v["Defender"] = await Ps("$s=Get-MpComputerStatus; if($s.AntivirusEnabled -and $s.RealTimeProtectionEnabled){'Enabled · Real-time protection on'}else{'Attention required'}");
        v["Firewall"] = await Ps("$p=Get-NetFirewallProfile; if(($p|Where-Object Enabled).Count -eq $p.Count){'All profiles enabled'}else{'Attention required'}");
        return new(v);
    }

    public static async Task<string> Ps(string script)
    {
        const string utf8Preamble = "$OutputEncoding=[Console]::OutputEncoding=[Text.Encoding]::UTF8;";
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(utf8Preamble + "$ErrorActionPreference='Stop';" + script));
        var r = await RunAsync("powershell.exe", $"-NoProfile -NonInteractive -EncodedCommand {encoded}");
        return r.Success ? r.Output.Trim() : "Unavailable";
    }

    public static void Open(string file, string args = "") => Process.Start(new ProcessStartInfo(file, args) { UseShellExecute = true });
    public static string Format(long b) { string[] u = ["B", "KB", "MB", "GB", "TB"]; double x = b; int i = 0; while (x >= 1024 && i < 4) { x /= 1024; i++; } return $"{x:0.##} {u[i]}"; }

    internal static string GetWindowsDisplayName()
    {
        const string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        var productName = Registry.GetValue(keyPath, "ProductName", "Windows")?.ToString() ?? "Windows";
        var displayVersion = Registry.GetValue(keyPath, "DisplayVersion", null)?.ToString();
        var buildText = Registry.GetValue(keyPath, "CurrentBuildNumber", null)?.ToString();
        var revisionText = Registry.GetValue(keyPath, "UBR", null)?.ToString();

        if (!int.TryParse(buildText, out var build))
            build = Environment.OSVersion.Version.Build;

        // Windows 11 keeps the NT 10.0 version and upgraded systems can retain
        // "Windows 10" in ProductName. Build 22000 is the reliable family boundary.
        if (build >= 22000 && productName.Contains("Windows 10", StringComparison.OrdinalIgnoreCase))
            productName = productName.Replace("Windows 10", "Windows 11", StringComparison.OrdinalIgnoreCase);

        var versionPart = string.IsNullOrWhiteSpace(displayVersion) ? "" : $" · {displayVersion}";
        var buildPart = string.IsNullOrWhiteSpace(revisionText) ? build.ToString(CultureInfo.InvariantCulture) : $"{build}.{revisionText}";
        return $"{productName}{versionPart} · build {buildPart}";
    }
}
