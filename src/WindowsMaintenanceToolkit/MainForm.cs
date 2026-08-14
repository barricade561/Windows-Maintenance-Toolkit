using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace WindowsMaintenanceToolkit;

public sealed class MainForm : Form
{
    private readonly Color _bg = Color.FromArgb(13, 17, 23);
    private readonly Color _panel = Color.FromArgb(22, 27, 34);
    private readonly Color _panel2 = Color.FromArgb(30, 36, 45);
    private readonly Color _text = Color.FromArgb(230, 237, 243);
    private readonly Color _muted = Color.FromArgb(139, 148, 158);
    private readonly Color _accent = Color.FromArgb(47, 129, 247);
    private readonly Color _success = Color.FromArgb(46, 160, 67);

    private readonly RichTextBox _log = new();
    private readonly Label _status = new();
    private readonly ProgressBar _progress = new();
    private readonly FlowLayoutPanel _actions = new();
    private bool _busy;

    public MainForm()
    {
        Text = "Windows Maintenance Toolkit";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 720);
        Size = new Size(1180, 800);
        BackColor = _bg;
        ForeColor = _text;
        Font = new Font("Segoe UI", 10F);

        BuildUi();
        AppendLog("Toolkit ready. Administrator privileges are enabled by the application manifest.");
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(24),
            BackColor = _bg
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        Controls.Add(root);

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildActionArea(), 0, 1);
        root.Controls.Add(BuildConsole(), 0, 2);
        root.Controls.Add(BuildFooter(), 0, 3);
    }

    private Control BuildHeader()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = _bg };

        var title = new Label
        {
            Text = "Windows Maintenance Toolkit",
            AutoSize = true,
            ForeColor = _text,
            Font = new Font("Segoe UI Semibold", 22F),
            Location = new Point(4, 5)
        };
        panel.Controls.Add(title);

        var subtitle = new Label
        {
            Text = "Repair, diagnostics and maintenance tools for Windows 10 / 11",
            AutoSize = true,
            ForeColor = _muted,
            Font = new Font("Segoe UI", 10.5F),
            Location = new Point(7, 52)
        };
        panel.Controls.Add(subtitle);

        var admin = new Label
        {
            Text = "● Administrator",
            AutoSize = true,
            ForeColor = _success,
            Font = new Font("Segoe UI Semibold", 10F),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(870, 14)
        };
        panel.Resize += (_, _) => admin.Left = Math.Max(10, panel.ClientSize.Width - admin.Width - 8);
        panel.Controls.Add(admin);

        return panel;
    }

    private Control BuildActionArea()
    {
        var container = new Panel { Dock = DockStyle.Fill, BackColor = _bg };

        _actions.Dock = DockStyle.Fill;
        _actions.AutoScroll = true;
        _actions.WrapContents = true;
        _actions.FlowDirection = FlowDirection.LeftToRight;
        _actions.Padding = new Padding(0, 2, 0, 0);
        _actions.BackColor = _bg;

        AddAction("SFC Scan", "Scans and repairs protected Windows system files.", "sfc /scannow", async () => await RunCommandAsync("sfc.exe", "/scannow", "SFC Scan"));
        AddAction("DISM CheckHealth", "Checks whether the component store is flagged as corrupted.", "DISM /CheckHealth", async () => await RunCommandAsync("dism.exe", "/Online /Cleanup-Image /CheckHealth", "DISM CheckHealth"));
        AddAction("DISM ScanHealth", "Performs a deeper component-store corruption scan.", "DISM /ScanHealth", async () => await RunCommandAsync("dism.exe", "/Online /Cleanup-Image /ScanHealth", "DISM ScanHealth"));
        AddAction("DISM RestoreHealth", "Repairs the Windows component store using Windows servicing.", "DISM /RestoreHealth", async () => await RunCommandAsync("dism.exe", "/Online /Cleanup-Image /RestoreHealth", "DISM RestoreHealth"));
        AddAction("Check Disk", "Runs the online NTFS scan without forcing a reboot.", "chkdsk C: /scan", async () => await RunCommandAsync("chkdsk.exe", "C: /scan", "CHKDSK Online Scan"));
        AddAction("Flush DNS", "Clears the local DNS resolver cache.", "ipconfig /flushdns", async () => await RunCommandAsync("ipconfig.exe", "/flushdns", "DNS Cache Flush"));
        AddAction("Reset Winsock", "Resets the Winsock catalog. A restart may be required.", "netsh winsock reset", async () => await RunCommandAsync("netsh.exe", "winsock reset", "Winsock Reset"));
        AddAction("Temp Cleanup", "Removes deletable files from user and Windows temp folders.", "Safe temp cleanup", async () => await CleanupTempAsync());
        AddAction("System Info", "Displays OS, memory, CPU and system-drive information.", "Local diagnostics", async () => await ShowSystemInfoAsync());
        AddAction("Full Health Check", "Runs DISM CheckHealth, DISM ScanHealth, SFC and CHKDSK in sequence.", "Recommended diagnostic sequence", async () => await RunFullHealthCheckAsync(), true);

        container.Controls.Add(_actions);
        return container;
    }

    private void AddAction(string title, string description, string command, Func<Task> action, bool featured = false)
    {
        var card = new Panel
        {
            Width = 340,
            Height = 155,
            Margin = new Padding(0, 0, 14, 14),
            Padding = new Padding(16),
            BackColor = featured ? Color.FromArgb(25, 45, 75) : _panel
        };

        var lblTitle = new Label
        {
            Text = title,
            AutoSize = true,
            ForeColor = _text,
            Font = new Font("Segoe UI Semibold", 12F),
            Location = new Point(16, 14)
        };
        card.Controls.Add(lblTitle);

        var lblDesc = new Label
        {
            Text = description,
            ForeColor = _muted,
            Font = new Font("Segoe UI", 9F),
            Location = new Point(16, 45),
            Size = new Size(305, 42)
        };
        card.Controls.Add(lblDesc);

        var lblCmd = new Label
        {
            Text = command,
            ForeColor = Color.FromArgb(166, 203, 255),
            Font = new Font("Cascadia Mono", 8.5F),
            Location = new Point(16, 91),
            Size = new Size(205, 30)
        };
        card.Controls.Add(lblCmd);

        var button = new Button
        {
            Text = featured ? "RUN ALL" : "RUN",
            Size = new Size(92, 34),
            Location = new Point(230, 101),
            FlatStyle = FlatStyle.Flat,
            BackColor = featured ? _success : _accent,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 9F),
            TabStop = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += async (_, _) => await ExecuteActionAsync(action);
        card.Controls.Add(button);

        _actions.Controls.Add(card);
    }

    private Control BuildConsole()
    {
        var frame = new Panel { Dock = DockStyle.Fill, BackColor = _panel, Padding = new Padding(1) };
        var inner = new Panel { Dock = DockStyle.Fill, BackColor = _panel2, Padding = new Padding(14) };
        frame.Controls.Add(inner);

        var caption = new Label
        {
            Text = "ACTIVITY LOG",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = _muted,
            Font = new Font("Segoe UI Semibold", 9F)
        };
        inner.Controls.Add(caption);

        _log.Dock = DockStyle.Fill;
        _log.BackColor = Color.FromArgb(11, 15, 20);
        _log.ForeColor = Color.FromArgb(201, 209, 217);
        _log.BorderStyle = BorderStyle.None;
        _log.ReadOnly = true;
        _log.Font = new Font("Cascadia Mono", 9.5F);
        _log.DetectUrls = false;
        inner.Controls.Add(_log);
        _log.BringToFront();

        return frame;
    }

    private Control BuildFooter()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = _bg };

        _status.Text = "Ready";
        _status.ForeColor = _muted;
        _status.AutoSize = true;
        _status.Location = new Point(2, 17);
        panel.Controls.Add(_status);

        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 25;
        _progress.Visible = false;
        _progress.Size = new Size(210, 6);
        _progress.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        _progress.Location = new Point(840, 20);
        panel.Resize += (_, _) => _progress.Left = Math.Max(10, panel.ClientSize.Width - _progress.Width - 4);
        panel.Controls.Add(_progress);

        return panel;
    }

    private async Task ExecuteActionAsync(Func<Task> action)
    {
        if (_busy) return;
        _busy = true;
        SetBusy(true);
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            AppendLog($"ERROR: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
            _busy = false;
        }
    }

    private async Task<int> RunCommandAsync(string fileName, string arguments, string displayName)
    {
        AppendLog($"\n=== {displayName} ===");
        AppendLog($"> {fileName} {arguments}");
        _status.Text = $"Running: {displayName}";

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) SafeAppend(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) SafeAppend("[stderr] " + e.Data); };

        if (!process.Start())
            throw new InvalidOperationException($"Could not start {fileName}.");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        AppendLog($"Exit code: {process.ExitCode}");
        AppendLog(process.ExitCode == 0 ? "Completed successfully." : "Completed with a non-zero exit code. Review the output above.");
        return process.ExitCode;
    }

    private async Task RunFullHealthCheckAsync()
    {
        AppendLog("\n######## FULL SYSTEM HEALTH CHECK ########");
        await RunCommandAsync("dism.exe", "/Online /Cleanup-Image /CheckHealth", "1/4 DISM CheckHealth");
        await RunCommandAsync("dism.exe", "/Online /Cleanup-Image /ScanHealth", "2/4 DISM ScanHealth");
        await RunCommandAsync("sfc.exe", "/scannow", "3/4 SFC Scan");
        await RunCommandAsync("chkdsk.exe", "C: /scan", "4/4 CHKDSK Online Scan");
        AppendLog("######## HEALTH CHECK FINISHED ########\n");
    }

    private async Task CleanupTempAsync()
    {
        _status.Text = "Cleaning temporary files";
        AppendLog("\n=== Temporary File Cleanup ===");

        var folders = new[]
        {
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
        }.Distinct(StringComparer.OrdinalIgnoreCase);

        int filesDeleted = 0;
        int dirsDeleted = 0;
        long bytesFreed = 0;

        await Task.Run(() =>
        {
            foreach (var folder in folders)
            {
                SafeAppend($"Scanning: {folder}");
                if (!Directory.Exists(folder)) continue;

                foreach (var file in EnumerateSafeFiles(folder))
                {
                    try
                    {
                        var info = new FileInfo(file);
                        long len = info.Exists ? info.Length : 0;
                        info.IsReadOnly = false;
                        info.Delete();
                        filesDeleted++;
                        bytesFreed += len;
                    }
                    catch { }
                }

                foreach (var dir in EnumerateSafeDirectories(folder).OrderByDescending(x => x.Length))
                {
                    try
                    {
                        Directory.Delete(dir, false);
                        dirsDeleted++;
                    }
                    catch { }
                }
            }
        });

        AppendLog($"Deleted files: {filesDeleted}");
        AppendLog($"Removed empty directories: {dirsDeleted}");
        AppendLog($"Approx. space freed: {FormatBytes(bytesFreed)}");
        AppendLog("Files currently in use were intentionally skipped.");
    }

    private static IEnumerable<string> EnumerateSafeFiles(string root)
    {
        try { return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToArray(); }
        catch
        {
            var result = new List<string>();
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                try { result.AddRange(Directory.EnumerateFiles(current)); } catch { }
                try { foreach (var d in Directory.EnumerateDirectories(current)) pending.Push(d); } catch { }
            }
            return result;
        }
    }

    private static IEnumerable<string> EnumerateSafeDirectories(string root)
    {
        var result = new List<string>();
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            try
            {
                foreach (var d in Directory.EnumerateDirectories(current))
                {
                    result.Add(d);
                    pending.Push(d);
                }
            }
            catch { }
        }
        return result;
    }

    private Task ShowSystemInfoAsync()
    {
        AppendLog("\n=== System Information ===");
        AppendLog($"Machine: {Environment.MachineName}");
        AppendLog($"User: {Environment.UserDomainName}\\{Environment.UserName}");
        AppendLog($"OS: {Environment.OSVersion}");
        AppendLog($"64-bit OS: {Environment.Is64BitOperatingSystem}");
        AppendLog($"Processors: {Environment.ProcessorCount}");
        AppendLog($".NET Runtime: {Environment.Version}");

        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)!);
            AppendLog($"System drive: {drive.Name} {FormatBytes(drive.AvailableFreeSpace)} free / {FormatBytes(drive.TotalSize)} total");
        }
        catch (Exception ex)
        {
            AppendLog($"Drive information unavailable: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private void SetBusy(bool busy)
    {
        _progress.Visible = busy;
        _status.ForeColor = busy ? Color.FromArgb(255, 193, 7) : _muted;
        if (!busy) _status.Text = "Ready";
        foreach (Control card in _actions.Controls)
            foreach (Control child in card.Controls)
                if (child is Button b) b.Enabled = !busy;
    }

    private void SafeAppend(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(text));
            return;
        }
        AppendLog(text);
    }

    private void AppendLog(string text)
    {
        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
        _log.SelectionStart = _log.TextLength;
        _log.ScrollToCaret();
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        int i = 0;
        while (value >= 1024 && i < units.Length - 1)
        {
            value /= 1024;
            i++;
        }
        return $"{value:0.##} {units[i]}";
    }
}
