using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Text;

namespace WindowsMaintenanceToolkit;

public sealed class MainForm : Form
{
    private readonly Color Bg = Color.FromArgb(7, 7, 10), Side = Color.FromArgb(12, 10, 14), Panel = Color.FromArgb(20, 16, 21);
    private readonly Color Crimson = Color.FromArgb(225, 18, 52), CrimsonDark = Color.FromArgb(92, 8, 25), TextColor = Color.FromArgb(244, 238, 241), Muted = Color.FromArgb(155, 139, 146);
    private readonly Panel _content = new(), _toolHost = new(), _dashboard = new();
    private readonly FlowLayoutPanel _nav = new(), _cards = new(), _stats = new();
    private readonly RichTextBox _log = new();
    private readonly Label _pageTitle = new(), _status = new();
    private readonly ProgressBar _progress = new();
    private readonly ComboBox _language = new();
    private readonly NotifyIcon _tray = new();
    private readonly AppSettings _settings;
    private readonly List<Button> _runButtons = [];
    private readonly List<ToolDefinition> _tools = [];
    private readonly string _logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Barracuda Systems", "Windows Maintenance Toolkit", "Logs");
    private string _activePage = "dashboard";
    private bool _busy, _allowClose;

    public MainForm()
    {
        _settings = AppSettings.Load(); L.Language = _settings.Language;
        Text = "Windows Maintenance Toolkit — Barracuda Systems"; MinimumSize = new(1160, 760); Size = new(1420, 900);
        StartPosition = FormStartPosition.CenterScreen; BackColor = Bg; ForeColor = TextColor; Font = new("Segoe UI", 9.5f);
        Icon = SystemIcons.Shield; DoubleBuffered = true;
        BuildTools(); BuildUi(); CleanupOldLogs(); Navigate("dashboard");
        Log(LogSeverity.Info, "Windows Maintenance Toolkit initialized — Barracuda Systems.");
        Shown += async (_, _) => await RefreshDashboardAsync();
        Resize += (_, _) => { if (_settings.MinimizeToTray && WindowState == FormWindowState.Minimized) { Hide(); _tray.Visible = true; } };
        FormClosing += (_, e) => { if (!_allowClose && _settings.MinimizeToTray) { e.Cancel = true; Hide(); _tray.Visible = true; } };
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Bg };
        root.ColumnStyles.Add(new(SizeType.Absolute, 235)); root.ColumnStyles.Add(new(SizeType.Percent, 100)); Controls.Add(root);
        root.Controls.Add(BuildSidebar(), 0, 0); root.Controls.Add(BuildMain(), 1, 0);
        _tray.Icon = SystemIcons.Shield; _tray.Text = Text; _tray.DoubleClick += (_, _) => { Show(); WindowState = FormWindowState.Normal; _tray.Visible = false; };
        _tray.ContextMenuStrip = new ContextMenuStrip(); _tray.ContextMenuStrip.Items.Add("Open", null, (_, _) => { Show(); WindowState = FormWindowState.Normal; });
        _tray.ContextMenuStrip.Items.Add("Exit", null, (_, _) => { _allowClose = true; Close(); });
    }

    private Control BuildSidebar()
    {
        var side = new Panel { Dock = DockStyle.Fill, BackColor = Side, Padding = new(14) };
        side.Controls.Add(new Label { Text = "WMT", ForeColor = Crimson, Font = new("Segoe UI Black", 28), AutoSize = true, Location = new(16, 14) });
        side.Controls.Add(new Label { Text = "WINDOWS MAINTENANCE\nTOOLKIT", ForeColor = TextColor, Font = new("Segoe UI Semibold", 9), AutoSize = true, Location = new(17, 66) });
        side.Controls.Add(new Label { Text = "BARRACUDA SYSTEMS", ForeColor = Crimson, Font = new("Segoe UI Semibold", 8), AutoSize = true, Location = new(17, 105) });
        _nav.Location = new(8, 145); _nav.Size = new(215, 655); _nav.FlowDirection = FlowDirection.TopDown; _nav.WrapContents = false; _nav.AutoScroll = true; _nav.BackColor = Side; side.Controls.Add(_nav);
        foreach (var key in Pages()) { var b = NavButton(key); _nav.Controls.Add(b); }
        return side;
    }

    private Control BuildMain()
    {
        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new(22, 18, 22, 16), BackColor = Bg };
        main.RowStyles.Add(new(SizeType.Absolute, 76)); main.RowStyles.Add(new(SizeType.Percent, 66)); main.RowStyles.Add(new(SizeType.Percent, 34));
        main.Controls.Add(BuildHeader(), 0, 0); main.Controls.Add(BuildContent(), 0, 1); main.Controls.Add(BuildLog(), 0, 2); return main;
    }

    private Control BuildHeader()
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Bg };
        _pageTitle.Font = new("Segoe UI Semibold", 22); _pageTitle.ForeColor = TextColor; _pageTitle.AutoSize = true; _pageTitle.Location = new(0, 2); p.Controls.Add(_pageTitle);
        var sub = new Label { Text = L.T("subtitle"), ForeColor = Muted, AutoSize = true, Location = new(2, 43) }; p.Controls.Add(sub);
        _language.DropDownStyle = ComboBoxStyle.DropDownList; _language.Items.AddRange(["English", "Türkçe"]); _language.SelectedIndex = L.Language == "tr" ? 1 : 0;
        _language.Size = new(112, 28); _language.Anchor = AnchorStyles.Top | AnchorStyles.Right; _language.Location = new(1000, 8); p.Controls.Add(_language);
        p.Resize += (_, _) => _language.Left = p.ClientSize.Width - _language.Width;
        _language.SelectedIndexChanged += (_, _) => { var lang = _language.SelectedIndex == 1 ? "tr" : "en"; if (lang == L.Language) return; L.Language = _settings.Language = lang; _settings.Save(); RebuildLocalizedUi(); };
        return p;
    }

    private Control BuildContent()
    {
        _content.Dock = DockStyle.Fill; _content.BackColor = Bg;
        _dashboard.Dock = DockStyle.Fill; _dashboard.BackColor = Bg; _content.Controls.Add(_dashboard);
        _toolHost.Dock = DockStyle.Fill; _toolHost.BackColor = Bg; _cards.Dock = DockStyle.Fill; _cards.AutoScroll = true; _cards.BackColor = Bg; _toolHost.Controls.Add(_cards); _content.Controls.Add(_toolHost);
        return _content;
    }

    private Control BuildLog()
    {
        var frame = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, Padding = new(12), BackColor = Panel };
        frame.RowStyles.Add(new(SizeType.Absolute, 26)); frame.RowStyles.Add(new(SizeType.Percent, 100)); frame.RowStyles.Add(new(SizeType.Absolute, 42));
        frame.Controls.Add(new Label { Name = "LogCaption", Text = L.T("activity"), ForeColor = Crimson, Dock = DockStyle.Fill, Font = new("Segoe UI Semibold", 9) }, 0, 0);
        _log.Dock = DockStyle.Fill; _log.BackColor = Color.FromArgb(8, 7, 9); _log.ForeColor = Color.Gainsboro; _log.BorderStyle = BorderStyle.None; _log.ReadOnly = true; _log.Font = new("Cascadia Mono", 9); frame.Controls.Add(_log, 0, 1);
        var foot = new Panel { Dock = DockStyle.Fill, BackColor = Panel };
        var clear = SmallButton(L.T("clear")); clear.Name = "ClearLog"; clear.Location = new(0, 7); clear.Click += (_, _) => _log.Clear(); foot.Controls.Add(clear);
        var export = SmallButton(L.T("export")); export.Name = "ExportLog"; export.Location = new(142, 7); export.Click += (_, _) => ExportLog(); foot.Controls.Add(export);
        _status.AutoSize = true; _status.Text = L.T("ready"); _status.ForeColor = Muted; _status.Location = new(300, 13); foot.Controls.Add(_status);
        _progress.Style = ProgressBarStyle.Marquee; _progress.MarqueeAnimationSpeed = 25; _progress.Visible = false; _progress.Size = new(180, 6); _progress.Anchor = AnchorStyles.Top | AnchorStyles.Right; foot.Controls.Add(_progress); foot.Resize += (_, _) => _progress.Location = new(foot.ClientSize.Width - 190, 16);
        frame.Controls.Add(foot, 0, 2); return frame;
    }

    private void BuildTools()
    {
        Tool("sfc", "system_repair", RiskLevel.Safe, f => f.Command("sfc.exe", "/scannow", "SFC Scan"), true);
        Tool("dism_check", "system_repair", RiskLevel.Safe, f => f.Command("dism.exe", "/Online /Cleanup-Image /CheckHealth", "DISM CheckHealth"));
        Tool("dism_scan", "system_repair", RiskLevel.Safe, f => f.Command("dism.exe", "/Online /Cleanup-Image /ScanHealth", "DISM ScanHealth"), true);
        Tool("dism_restore", "system_repair", RiskLevel.Caution, f => f.Command("dism.exe", "/Online /Cleanup-Image /RestoreHealth", "DISM RestoreHealth"), true);
        Tool("chkdsk", "diagnostics", RiskLevel.Safe, f => f.Command("chkdsk.exe", "C: /scan", "CHKDSK Online Scan"), true);
        Tool("full_health", "diagnostics", RiskLevel.Safe, f => f.FullHealthCheck(), true);
        Tool("temp_cleanup", "cleanup", RiskLevel.Caution, f => f.TempCleanup()); Tool("disk_cleanup", "cleanup", RiskLevel.Safe, f => f.Launch("cleanmgr.exe"));
        Tool("flush_dns", "network", RiskLevel.Safe, f => f.Command("ipconfig.exe", "/flushdns", "Flush DNS"));
        Tool("winsock", "network", RiskLevel.RestartRequired, f => f.Command("netsh.exe", "winsock reset", "Reset Winsock"));
        Tool("ping", "network", RiskLevel.Safe, f => f.Command("ping.exe", "-n 4 1.1.1.1", "Internet Ping"));
        Tool("trace", "network", RiskLevel.Safe, f => f.Command("tracert.exe", "-d -h 12 1.1.1.1", "Traceroute"));
        Tool("dns_test", "network", RiskLevel.Safe, f => f.Command("nslookup.exe", "microsoft.com", "DNS Test"));
        Tool("gateway", "network", RiskLevel.Safe, f => f.Command("ipconfig.exe", "/all", "Network & Gateway"));
        Tool("restore_point", "recovery", RiskLevel.Caution, f => f.CreateRestorePoint());
        Tool("startup_apps", "startup_services", RiskLevel.Safe, f => f.PowerShell("Get-CimInstance Win32_StartupCommand | Select-Object Name,Command,Location,User | Format-Table -AutoSize", "Startup Apps"));
        Tool("services", "startup_services", RiskLevel.Caution, f => f.Launch("services.msc"));
        Tool("update_repair", "windows_update", RiskLevel.RestartRequired, f => f.WindowsUpdateRepair(), true);
        Tool("update_settings", "windows_update", RiskLevel.Safe, f => f.Launch("ms-settings:windowsupdate"));
        Tool("balanced_power", "optimization", RiskLevel.Caution, f => f.Command("powercfg.exe", "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e", "Balanced Power Plan"));
        Tool("visual_effects", "optimization", RiskLevel.Caution, f => f.Launch("SystemPropertiesPerformance.exe"));
        Tool("event_viewer", "utilities", RiskLevel.Safe, f => f.Launch("eventvwr.msc")); Tool("task_manager", "utilities", RiskLevel.Safe, f => f.Launch("taskmgr.exe"));
        Tool("device_manager", "utilities", RiskLevel.Safe, f => f.Launch("devmgmt.msc")); Tool("task_scheduler", "utilities", RiskLevel.Safe, f => f.Launch("taskschd.msc"));
        Tool("defender", "security", RiskLevel.Safe, f => f.Launch("windowsdefender:")); Tool("firewall", "security", RiskLevel.Safe, f => f.Launch("wf.msc"));
        Tool("system_information", "system_info", RiskLevel.Safe, f => f.Launch("msinfo32.exe"));
    }

    private static IEnumerable<string> Pages() => ["dashboard", "system_repair", "cleanup", "network", "optimization", "diagnostics", "security", "system_info", "startup_services", "windows_update", "recovery", "utilities", "logs", "settings", "about"];
    private void Tool(string id, string category, RiskLevel risk, Func<MainForm, Task> action, bool longRunning = false) => _tools.Add(new(id, category, risk, action, longRunning));
    private Button NavButton(string key) { var b = new Button { Name = key, Text = L.T(key), Width = 196, Height = 38, Margin = new(0, 0, 0, 3), TextAlign = ContentAlignment.MiddleLeft, Padding = new(10, 0, 0, 0), FlatStyle = FlatStyle.Flat, BackColor = Side, ForeColor = Muted, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; b.Click += (_, _) => Navigate(key); return b; }
    private Button SmallButton(string text) { var b = new Button { Text = text, Size = new(132, 28), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(35, 25, 29), ForeColor = TextColor }; b.FlatAppearance.BorderColor = CrimsonDark; return b; }

    private void Navigate(string page)
    {
        _activePage = page; _pageTitle.Text = L.T(page);
        foreach (Button b in _nav.Controls) { bool on = b.Name == page; b.BackColor = on ? Color.FromArgb(48, 12, 22) : Side; b.ForeColor = on ? Color.White : Muted; b.FlatAppearance.BorderSize = on ? 1 : 0; b.FlatAppearance.BorderColor = Crimson; }
        _dashboard.Visible = page == "dashboard"; _toolHost.Visible = page != "dashboard";
        if (page == "dashboard") { BuildDashboardShell(); _ = RefreshDashboardAsync(); }
        else if (page == "settings") BuildSettings(); else if (page == "about") BuildAbout(); else if (page == "logs") BuildHistory(); else BuildCategory(page);
    }

    private void BuildDashboardShell()
    {
        _dashboard.Controls.Clear(); _stats.Controls.Clear(); _stats.Dock = DockStyle.Fill; _stats.AutoScroll = true; _stats.BackColor = Bg; _dashboard.Controls.Add(_stats);
        _stats.Controls.Add(new Label { Text = L.T("loading"), ForeColor = Muted, AutoSize = true, Margin = new(10) });
    }

    private async Task RefreshDashboardAsync()
    {
        try { var s = await SystemServices.SnapshotAsync(); if (IsDisposed) return; _stats.Controls.Clear(); foreach (var pair in s.Values) _stats.Controls.Add(InfoCard(pair.Key, pair.Value)); }
        catch (Exception ex) { Log(LogSeverity.Error, ex.Message); }
    }

    private Control InfoCard(string title, string value) => new Panel { Width = 255, Height = 88, Margin = new(0, 0, 12, 12), Padding = new(13), BackColor = Panel, Controls = { new Label { Text = title.ToUpperInvariant(), ForeColor = Crimson, Font = new("Segoe UI Semibold", 8), AutoSize = true, Location = new(13, 11) }, new Label { Text = value, ForeColor = TextColor, Font = new("Segoe UI", 10), AutoEllipsis = true, Location = new(13, 38), Size = new(226, 34) } } };

    private void BuildCategory(string category)
    {
        _cards.Controls.Clear(); _runButtons.Clear(); foreach (var tool in _tools.Where(t => t.Category == category)) _cards.Controls.Add(ToolCard(tool));
        if (_cards.Controls.Count == 0) _cards.Controls.Add(new Label { Text = "No tools in this section.", ForeColor = Muted, AutoSize = true });
    }

    private Control ToolCard(ToolDefinition tool)
    {
        var p = new Panel { Width = 330, Height = 142, BackColor = Panel, Margin = new(0, 0, 12, 12), Padding = new(14) };
        p.Controls.Add(new Label { Text = ToolName(tool.Id), ForeColor = TextColor, Font = new("Segoe UI Semibold", 12), AutoSize = true, Location = new(14, 12) });
        p.Controls.Add(new Label { Text = ToolDescription(tool.Id), ForeColor = Muted, Location = new(14, 42), Size = new(300, 45) });
        var risk = new Label { Text = RiskText(tool.Risk), ForeColor = tool.Risk == RiskLevel.Safe ? Color.FromArgb(80, 210, 140) : Color.FromArgb(255, 116, 80), Font = new("Segoe UI Semibold", 8), AutoSize = true, Location = new(14, 108) }; p.Controls.Add(risk);
        var run = SmallButton(L.T("run")); run.Size = new(76, 28); run.Location = new(238, 101); run.BackColor = Crimson; run.FlatAppearance.BorderSize = 0; run.Click += async (_, _) => await ExecuteTool(tool); p.Controls.Add(run); _runButtons.Add(run); return p;
    }

    private async Task ExecuteTool(ToolDefinition tool)
    {
        if (_busy) return;
        if (tool.Risk != RiskLevel.Safe && _settings.ConfirmRiskyOperations && MessageBox.Show(L.T("confirm"), ToolName(tool.Id), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        if (tool.Risk != RiskLevel.Safe && _settings.AutoRestorePoint && tool.Id != "restore_point") await TryAutoRestorePoint();
        _busy = true; SetBusy(true, ToolName(tool.Id)); try { await tool.Execute(this); } catch (Exception ex) { Log(LogSeverity.Error, ex.Message); } finally { _busy = false; SetBusy(false, ""); }
    }

    public async Task Command(string file, string args, string name)
    {
        Log(LogSeverity.Info, $"> {file} {args}"); var r = await SystemServices.RunAsync(file, args, x => SafeLog(LogSeverity.Info, x)); Log(r.Success ? LogSeverity.Success : LogSeverity.Error, $"{name} completed. Exit code: {r.ExitCode}", r.ExitCode);
    }
    public async Task PowerShell(string script, string name) { var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script)); await Command("powershell.exe", $"-NoProfile -EncodedCommand {encoded}", name); }
    public Task Launch(string target) { SystemServices.Open(target); Log(LogSeverity.Success, $"Opened: {target}"); return Task.CompletedTask; }

    public async Task CreateRestorePoint() => await PowerShell("Enable-ComputerRestore -Drive $env:SystemDrive; Checkpoint-Computer -Description 'Windows Maintenance Toolkit' -RestorePointType MODIFY_SETTINGS", "Restore Point");
    private async Task TryAutoRestorePoint() { try { Log(LogSeverity.Info, "Creating safety restore point…"); await CreateRestorePoint(); } catch (Exception ex) { Log(LogSeverity.Warning, "Restore point could not be created: " + ex.Message); } }
    public async Task WindowsUpdateRepair()
    {
        foreach (var svc in new[] { "wuauserv", "bits", "cryptsvc" }) await Command("net.exe", $"stop {svc}", $"Stop {svc}");
        await PowerShell("Remove-Item -Path \"$env:windir\\SoftwareDistribution\\Download\\*\" -Recurse -Force -ErrorAction SilentlyContinue", "Clear Windows Update cache");
        foreach (var svc in new[] { "cryptsvc", "bits", "wuauserv" }) await Command("net.exe", $"start {svc}", $"Start {svc}");
    }
    public async Task TempCleanup()
    {
        long bytes = 0; int files = 0; var roots = new[] { Path.GetTempPath(), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp") }.Distinct();
        await Task.Run(() => { foreach (var root in roots) { if (!Directory.Exists(root)) continue; foreach (var file in EnumerateFiles(root)) try { var i = new FileInfo(file); bytes += i.Length; i.IsReadOnly = false; i.Delete(); files++; } catch { } } });
        Log(LogSeverity.Success, $"Temporary cleanup: {files} files, approximately {SystemServices.Format(bytes)} freed.");
    }
    private static IEnumerable<string> EnumerateFiles(string root) { var q = new Stack<string>(); q.Push(root); while (q.Count > 0) { var d = q.Pop(); string[] files = [], dirs = []; try { files = Directory.GetFiles(d); } catch { } foreach (var f in files) yield return f; try { dirs = Directory.GetDirectories(d); } catch { } foreach (var x in dirs) q.Push(x); } }
    public async Task FullHealthCheck()
    {
        var checks = new List<HealthCheckResult>();
        foreach (var item in new[] { ("DISM CheckHealth", "dism.exe", "/Online /Cleanup-Image /CheckHealth"), ("DISM ScanHealth", "dism.exe", "/Online /Cleanup-Image /ScanHealth"), ("SFC", "sfc.exe", "/scannow"), ("CHKDSK", "chkdsk.exe", "C: /scan") })
        { Log(LogSeverity.Info, "Checking " + item.Item1); var r = await SystemServices.RunAsync(item.Item2, item.Item3, x => SafeLog(LogSeverity.Info, x)); checks.Add(new(item.Item1, r.Success, $"Exit code {r.ExitCode}")); }
        var failed = checks.Where(c => !c.Passed).ToList(); Log(failed.Count == 0 ? LogSeverity.Success : LogSeverity.Warning, failed.Count == 0 ? "HEALTHY — all real checks passed. " + L.T("no_percent") : $"ATTENTION REQUIRED — {failed.Count} check(s) reported issues: {string.Join(", ", failed.Select(f => f.Name))}. " + L.T("no_percent"));
    }

    private void BuildSettings()
    {
        _cards.Controls.Clear(); var p = new FlowLayoutPanel { Width = 650, Height = 340, FlowDirection = FlowDirection.TopDown, BackColor = Panel, Padding = new(20), WrapContents = false };
        Check(p, "Minimize to tray", _settings.MinimizeToTray, v => _settings.MinimizeToTray = v); Check(p, "Confirm risky operations", _settings.ConfirmRiskyOperations, v => _settings.ConfirmRiskyOperations = v);
        Check(p, "Create restore point before risky operations", _settings.AutoRestorePoint, v => _settings.AutoRestorePoint = v); Check(p, "Animations / progress indicators", _settings.Animations, v => _settings.Animations = v);
        p.Controls.Add(new Label { Text = "Log retention (days)", ForeColor = TextColor, AutoSize = true, Margin = new(3, 12, 3, 3) });
        var days = new NumericUpDown { Minimum = 1, Maximum = 365, Value = _settings.LogRetentionDays, Width = 100, BackColor = Side, ForeColor = TextColor }; days.ValueChanged += (_, _) => { _settings.LogRetentionDays = (int)days.Value; _settings.Save(); }; p.Controls.Add(days); _cards.Controls.Add(p);
    }
    private void Check(Control p, string text, bool value, Action<bool> set) { var c = new CheckBox { Text = text, Checked = value, ForeColor = TextColor, AutoSize = true, Margin = new(3, 8, 3, 8) }; c.CheckedChanged += (_, _) => { set(c.Checked); _settings.Save(); }; p.Controls.Add(c); }
    private void BuildAbout() { _cards.Controls.Clear(); _cards.Controls.Add(new Label { Text = "WINDOWS MAINTENANCE TOOLKIT\n\nBarracuda Systems\nVersion 2.0.0 · .NET 8 · Windows x64\n\ngithub.com/barricade561/Windows-Maintenance-Toolkit\n\nSafe, transparent Windows maintenance. No random debloat.\nNo synthetic health percentages.", ForeColor = TextColor, Font = new("Segoe UI", 12), AutoSize = true, Padding = new(20), BackColor = Panel }); }
    private void BuildHistory() { _cards.Controls.Clear(); Directory.CreateDirectory(_logFolder); foreach (var file in Directory.GetFiles(_logFolder, "*.log").OrderByDescending(File.GetLastWriteTime).Take(50)) { var b = SmallButton(Path.GetFileName(file)); b.Width = 320; b.Click += (_, _) => SystemServices.Open(file); _cards.Controls.Add(b); } }

    private void SetBusy(bool busy, string name) { foreach (var b in _runButtons) b.Enabled = !busy; _progress.Visible = busy && _settings.Animations; _status.Text = busy ? $"{L.T("running")}: {name}" : L.T("ready"); _status.ForeColor = busy ? Crimson : Muted; }
    public void Log(LogSeverity severity, string message, int? exitCode = null) { var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{severity.ToString().ToUpperInvariant()}] {message}{(exitCode.HasValue ? $" [exit={exitCode}]" : "")}"; _log.AppendText(line + Environment.NewLine); _log.SelectionStart = _log.TextLength; _log.ScrollToCaret(); Persist(line); }
    private void SafeLog(LogSeverity s, string m) { if (InvokeRequired) BeginInvoke(() => Log(s, m)); else Log(s, m); }
    private void Persist(string line) { try { Directory.CreateDirectory(_logFolder); File.AppendAllText(Path.Combine(_logFolder, $"wmt-{DateTime.Now:yyyy-MM-dd}.log"), line + Environment.NewLine); } catch { } }
    private void CleanupOldLogs() { try { if (!Directory.Exists(_logFolder)) return; foreach (var f in Directory.GetFiles(_logFolder, "*.log").Where(f => File.GetLastWriteTimeUtc(f) < DateTime.UtcNow.AddDays(-_settings.LogRetentionDays))) File.Delete(f); } catch { } }
    private void ExportLog() { using var d = new SaveFileDialog { Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt", FileName = $"WMT-{DateTime.Now:yyyyMMdd-HHmm}.log" }; if (d.ShowDialog() == DialogResult.OK) { File.WriteAllText(d.FileName, _log.Text); Log(LogSeverity.Success, "Log exported: " + d.FileName); } }
    private void RebuildLocalizedUi() { foreach (Button b in _nav.Controls) b.Text = L.T(b.Name); var logCaption = Controls.Find("LogCaption", true).FirstOrDefault(); if (logCaption != null) logCaption.Text = L.T("activity"); var clear = Controls.Find("ClearLog", true).FirstOrDefault(); if (clear != null) clear.Text = L.T("clear"); var export = Controls.Find("ExportLog", true).FirstOrDefault(); if (export != null) export.Text = L.T("export"); Navigate(_activePage); }
    private string RiskText(RiskLevel r) => r switch { RiskLevel.Safe => L.T("safe"), RiskLevel.Caution => L.T("caution"), _ => L.T("restart") };
    private static string ToolName(string id) => id.Replace('_', ' ') switch { "sfc" => "SFC Scan", "dism check" => "DISM CheckHealth", "dism scan" => "DISM ScanHealth", "dism restore" => "DISM RestoreHealth", "chkdsk" => "CHKDSK Online Scan", "full health" => "Full Health Check", "temp cleanup" => "Temporary File Cleanup", "disk cleanup" => "Disk Cleanup", "winsock" => "Reset Winsock", "ping" => "Internet Ping", "trace" => "Traceroute", "gateway" => "Gateway & Adapter Info", "restore point" => "Create Restore Point", "services" => "Services Console", "defender" => "Windows Security", "firewall" => "Advanced Firewall", _ => CultureTitle(id.Replace('_', ' ')) };
    private static string CultureTitle(string s) => System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s);
    private static string ToolDescription(string id) => id switch { "full_health" => "Runs DISM, SFC and CHKDSK; reports only evidence-backed outcomes.", "update_repair" => "Restarts update services and safely clears the download cache.", "services" => "Opens the controlled Windows Services console; no critical service is disabled automatically.", "startup_apps" => "Lists configured startup entries without changing them.", "temp_cleanup" => "Deletes removable temporary files and skips locked files.", _ => "Runs the standard Windows tool transparently and records its result." };
}
