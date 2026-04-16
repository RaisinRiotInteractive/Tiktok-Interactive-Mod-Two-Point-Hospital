using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Color = System.Drawing.Color;
using Font  = System.Drawing.Font;

namespace TPH_TikTokInstaller;

public class InstallerForm : Form
{
    // ── Palette ───────────────────────────────────────────────────────
    static readonly Color ColBg     = Color.FromArgb(30,  31,  34);
    static readonly Color ColHeader = Color.FromArgb(38,  40,  45);
    static readonly Color ColInput  = Color.FromArgb(55,  57,  63);
    static readonly Color ColSep    = Color.FromArgb(55,  57,  63);
    static readonly Color ColText   = Color.FromArgb(219, 222, 225);
    static readonly Color ColMuted  = Color.FromArgb(128, 132, 142);
    static readonly Color ColAccent = Color.FromArgb(88,  101, 242);
    static readonly Color ColGreen  = Color.FromArgb(35,  165, 90);
    static readonly Color ColYellow = Color.FromArgb(240, 177, 50);
    static readonly Color ColRed    = Color.FromArgb(242, 63,  67);

    // ── Controls ──────────────────────────────────────────────────────
    private readonly TextBox _txtPath;
    private readonly Button  _btnBrowse, _btnDetect, _btnInstall;
    private readonly Panel   _pnlProgressTrack, _pnlProgressFill;
    private readonly Label   _lblStatus;
    private readonly Label[] _stepDots  = new Label[4];
    private readonly Label[] _stepTexts = new Label[4];

    // BepInEx 5.x stable release (x64)
    private const string BepInExUrl =
        "https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.2/BepInEx_win_x64_5.4.23.2.zip";

    private static readonly string[] StepTitles =
    {
        "Two Point Hospital",
        "BepInEx 5.x  (mod loader)",
        "TikTok Live Mod",
        "Companion App"
    };

    private enum StepState { Pending, Running, Done, Skipped, Error }

    private bool _installing;
    private static readonly HttpClient _http = new();

    // ── Dark title bar ────────────────────────────────────────────────
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int val, int size);

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        int v = 1;
        DwmSetWindowAttribute(Handle, 20, ref v, sizeof(int));
    }

    // ── Constructor ───────────────────────────────────────────────────
    public InstallerForm()
    {
        Text            = "TikTok Live Mod — Installer";
        ClientSize      = new Size(540, 456);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = ColBg;
        ForeColor       = ColText;
        Font            = new Font("Segoe UI", 9.5f);

        // ── Header ────────────────────────────────────────────────────
        var pnlHeader = new Panel { Location = new Point(0, 0), Size = new Size(540, 72), BackColor = ColHeader };
        pnlHeader.Paint += (_, e) =>
        {
            using var b = new SolidBrush(ColAccent);
            e.Graphics.FillRectangle(b, 0, 0, pnlHeader.Width, 3);
        };
        pnlHeader.Controls.Add(Lbl("TikTok Live Mod  ·  Two Point Hospital",
            new Font("Segoe UI", 14f, FontStyle.Bold), ColText, new Point(20, 12), new Size(500, 30)));
        pnlHeader.Controls.Add(Lbl("Interactive mod installer  ·  v1.2.5  by RaisinRiotInteractive",
            new Font("Segoe UI", 9f), ColMuted, new Point(21, 43), new Size(500, 18)));

        // ── Path row ──────────────────────────────────────────────────
        var lblPathHint = Lbl("Game installation folder:", null, ColMuted, new Point(20, 88), new Size(220, 18));

        _txtPath = new TextBox
        {
            Location    = new Point(20, 108),
            Size        = new Size(326, 24),
            BackColor   = ColInput,
            ForeColor   = ColText,
            BorderStyle = BorderStyle.FixedSingle,
            Font        = new Font("Segoe UI", 9.5f)
        };

        _btnBrowse = NavBtn("Browse…",       new Point(352, 107), new Size(74, 26));
        _btnDetect = NavBtn("Auto-Detect",   new Point(432, 107), new Size(88, 26));
        _btnBrowse.Click += BtnBrowse_Click;
        _btnDetect.Click += (_, _) => TryAutoDetect();

        // ── Separator ─────────────────────────────────────────────────
        var sep1 = Sep(new Point(20, 142));

        // ── Step rows ─────────────────────────────────────────────────
        for (int i = 0; i < 4; i++)
        {
            int y = 154 + i * 38;
            _stepDots[i]  = Lbl("○", new Font("Segoe UI", 13f), ColMuted,
                                 new Point(20, y), new Size(26, 28), ContentAlignment.MiddleCenter);
            _stepTexts[i] = Lbl(StepTitles[i], new Font("Segoe UI", 10f), ColMuted,
                                 new Point(52, y), new Size(468, 28), ContentAlignment.MiddleLeft);
        }

        var sep2 = Sep(new Point(20, 312));

        // ── Progress ──────────────────────────────────────────────────
        _pnlProgressTrack = new Panel
        {
            Location  = new Point(20, 324),
            Size      = new Size(500, 8),
            BackColor = Color.FromArgb(50, 52, 58)
        };
        _pnlProgressFill = new Panel { Location = new Point(0, 0), Size = new Size(0, 8), BackColor = ColAccent };
        _pnlProgressTrack.Controls.Add(_pnlProgressFill);

        _lblStatus = Lbl("Ready to install.", null, ColMuted, new Point(20, 338), new Size(500, 20));

        var sep3 = Sep(new Point(20, 366));

        // ── Install button ────────────────────────────────────────────
        _btnInstall = new Button
        {
            Text      = "Install",
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            Location  = new Point(150, 383),
            Size      = new Size(240, 44),
            BackColor = ColAccent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnInstall.FlatAppearance.BorderSize         = 0;
        _btnInstall.FlatAppearance.MouseOverBackColor = Color.FromArgb(108, 120, 255);
        _btnInstall.FlatAppearance.MouseDownBackColor = Color.FromArgb(65,  78, 210);
        _btnInstall.Click += BtnInstall_Click;

        // ── Wire up ───────────────────────────────────────────────────
        Controls.Add(pnlHeader);
        Controls.Add(lblPathHint);
        Controls.Add(_txtPath);
        Controls.Add(_btnBrowse);
        Controls.Add(_btnDetect);
        Controls.Add(sep1);
        for (int i = 0; i < 4; i++) { Controls.Add(_stepDots[i]); Controls.Add(_stepTexts[i]); }
        Controls.Add(sep2);
        Controls.Add(_pnlProgressTrack);
        Controls.Add(_lblStatus);
        Controls.Add(sep3);
        Controls.Add(_btnInstall);

        TryAutoDetect();
    }

    // ── Helper control factories ──────────────────────────────────────

    private static Label Lbl(string text, Font? font, Color fore, Point loc, Size size,
        ContentAlignment align = ContentAlignment.MiddleLeft)
        => new()
        {
            Text      = text,
            Font      = font ?? new Font("Segoe UI", 9.5f),
            ForeColor = fore,
            BackColor = Color.Transparent,
            Location  = loc,
            Size      = size,
            TextAlign = align,
            AutoSize  = false
        };

    private static Button NavBtn(string text, Point loc, Size size)
    {
        var b = new Button
        {
            Text      = text,
            Location  = loc,
            Size      = size,
            BackColor = Color.FromArgb(54, 57, 63),
            ForeColor = ColText,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            Font      = new Font("Segoe UI", 8.5f)
        };
        b.FlatAppearance.BorderColor        = Color.FromArgb(70, 73, 82);
        b.FlatAppearance.BorderSize         = 1;
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 68, 78);
        return b;
    }

    private Panel Sep(Point loc)
        => new() { Location = loc, Size = new Size(500, 1), BackColor = ColSep };

    // ── Auto-detect ───────────────────────────────────────────────────

    private void TryAutoDetect()
    {
        string? path = DetectTPHPath();
        if (path != null)
        {
            _txtPath.Text = path;
            SetStatus("Two Point Hospital detected automatically.");
        }
        else
        {
            SetStatus("Could not auto-detect. Please click Browse and select your TPH folder.");
        }
    }

    private static string? DetectTPHPath()
    {
        // 1. Steam App uninstall registry key (most reliable)
        foreach (string hive in new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 535930",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 535930"
        })
        {
            try
            {
                using var k = Registry.LocalMachine.OpenSubKey(hive);
                if (k?.GetValue("InstallLocation") is string loc && Directory.Exists(loc))
                    return loc;
            }
            catch { }
        }

        // 2. Scan Steam library folders via libraryfolders.vdf
        try
        {
            using var steamKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (steamKey?.GetValue("SteamPath") is string steamPath)
            {
                var libs = new System.Collections.Generic.List<string> { steamPath };
                string vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (File.Exists(vdf))
                    foreach (Match m in Regex.Matches(File.ReadAllText(vdf), @"""path""\s+""([^""]+)"""))
                        libs.Add(m.Groups[1].Value.Replace(@"\\", @"\"));

                foreach (string lib in libs)
                {
                    string candidate = Path.Combine(lib, "steamapps", "common", "TPH");
                    if (Directory.Exists(candidate)) return candidate;
                }
            }
        }
        catch { }

        // 3. Common fallback paths
        string[] fallbacks =
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\TPH",
            @"C:\Program Files\Steam\steamapps\common\TPH",
            @"D:\Steam\steamapps\common\TPH",
            @"D:\SteamLibrary\steamapps\common\TPH",
            @"E:\SteamLibrary\steamapps\common\TPH",
        };
        return Array.Find(fallbacks, Directory.Exists);
    }

    // ── Browse ────────────────────────────────────────────────────────

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description            = "Select your Two Point Hospital folder (the one containing TPH.exe)",
            UseDescriptionForTitle = true,
            SelectedPath           = _txtPath.Text.Trim()
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
            _txtPath.Text = dlg.SelectedPath;
    }

    // ── Install ───────────────────────────────────────────────────────

    private async void BtnInstall_Click(object? sender, EventArgs e)
    {
        if (_installing) return;

        string tphPath = _txtPath.Text.Trim();
        if (!File.Exists(Path.Combine(tphPath, "TPH.exe")))
        {
            MessageBox.Show(
                "Two Point Hospital was not found at the selected path.\n\n" +
                "Please choose the folder that contains TPH.exe.",
                "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _installing         = true;
        _btnInstall.Enabled = false;
        _btnInstall.Text    = "Installing…";
        ResetSteps();

        try
        {
            // ── Step 1: Verify game ───────────────────────────────────
            SetStep(0, StepState.Running, "Verifying installation…");
            await Task.Delay(200);
            SetStep(0, StepState.Done, $"Two Point Hospital  —  {tphPath}");
            SetProgress(8);

            // ── Step 2: BepInEx ───────────────────────────────────────
            string bepDll = Path.Combine(tphPath, "BepInEx", "core", "BepInEx.dll");
            if (File.Exists(bepDll))
            {
                SetStep(1, StepState.Skipped, "BepInEx already installed — skipped");
                SetProgress(40);
            }
            else
            {
                SetStep(1, StepState.Running, "Downloading BepInEx 5.4.23.2…");
                await DownloadBepInEx(tphPath);
                SetStep(1, StepState.Done, "BepInEx 5.4.23.2 installed");
                SetProgress(50);
            }

            // ── Step 3: Mod DLL ───────────────────────────────────────
            SetStep(2, StepState.Running, "Installing mod…");
            string pluginsDir = Path.Combine(tphPath, "BepInEx", "plugins");
            Directory.CreateDirectory(pluginsDir);
            string destDll    = Path.Combine(pluginsDir, "TPH_TikTokMod.dll");
            bool   modUpdate  = File.Exists(destDll);
            ExtractResource("TPH_TikTokMod.dll", destDll);
            SetStep(2, StepState.Done, modUpdate ? "TikTok Mod updated" : "TikTok Mod installed");
            SetProgress(75);

            // ── Step 4: Companion app ─────────────────────────────────
            SetStep(3, StepState.Running, "Installing Companion App…");
            string companionDir  = Path.Combine(tphPath, "TPH_TikTokCompanion");
            Directory.CreateDirectory(companionDir);
            string destExe       = Path.Combine(companionDir, "TPH_TikTokCompanion.exe");
            bool   compUpdate    = File.Exists(destExe);
            ExtractResource("TPH_TikTokCompanion.exe", destExe);
            SetStep(3, StepState.Done, compUpdate ? "Companion App updated" : "Companion App installed");
            SetProgress(100);

            // ── Done ──────────────────────────────────────────────────
            _lblStatus.ForeColor = ColGreen;
            SetStatus("Installation complete!");
            _btnInstall.Text      = "Done  ✓";
            _btnInstall.BackColor = ColGreen;
            _btnInstall.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 185, 100);

            MessageBox.Show(
                "All done!\n\n" +
                "To get started:\n" +
                "  1. Launch Two Point Hospital\n" +
                "  2. Run TPH_TikTokCompanion.exe from your TPH folder\n" +
                "  3. Enter your TikTok username and click Connect",
                "Installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            // Mark any still-running step as error
            for (int i = 0; i < 4; i++)
                if (_stepDots[i].Text == "◌") SetStep(i, StepState.Error, _stepTexts[i].Text);

            _lblStatus.ForeColor = ColRed;
            SetStatus($"Error: {ex.Message}");
            MessageBox.Show($"Installation failed:\n\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            _btnInstall.Text    = "Retry";
            _btnInstall.Enabled = true;
            _installing         = false;
        }
    }

    // ── BepInEx download ──────────────────────────────────────────────

    private async Task DownloadBepInEx(string tphPath)
    {
        using var response = await _http.GetAsync(BepInExUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        long total  = response.Content.Headers.ContentLength ?? 1;
        using var ms  = new MemoryStream();
        using var src = await response.Content.ReadAsStreamAsync();
        var buf = new byte[65536];
        long read = 0; int chunk;

        while ((chunk = await src.ReadAsync(buf)) > 0)
        {
            ms.Write(buf, 0, chunk);
            read += chunk;
            int pct = 10 + (int)(read * 30 / total);
            SetProgress(pct);
            SetStatus($"Downloading BepInEx…  {read / 1024}  /  {total / 1024} KB");
        }

        SetStatus("Extracting BepInEx…");
        SetProgress(41);
        ms.Position = 0;

        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
        foreach (var entry in zip.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;
            string dest = Path.Combine(tphPath, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            using var fs = File.Create(dest);
            using var es = entry.Open();
            await es.CopyToAsync(fs);
        }
    }

    // ── Embedded resource extraction ──────────────────────────────────

    private static void ExtractResource(string name, string destPath)
    {
        var asm  = Assembly.GetExecutingAssembly();
        var key  = $"TPH_TikTokInstaller.Resources.{name}";
        using var src = asm.GetManifestResourceStream(key)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{name}' is missing from this installer build.\n" +
                "Please download a fresh copy from the GitHub releases page.");
        using var dst = File.Create(destPath);
        src.CopyTo(dst);
    }

    // ── UI helpers ────────────────────────────────────────────────────

    private void ResetSteps()
    {
        for (int i = 0; i < 4; i++) SetStep(i, StepState.Pending, StepTitles[i]);
        SetProgress(0);
        _lblStatus.ForeColor = ColMuted;
        SetStatus("Installing…");
    }

    private void SetStep(int i, StepState state, string text)
    {
        if (InvokeRequired) { Invoke(() => SetStep(i, state, text)); return; }
        (_stepDots[i].Text, _stepDots[i].ForeColor) = state switch
        {
            StepState.Running => ("◌", ColYellow),
            StepState.Done    => ("✓", ColGreen),
            StepState.Skipped => ("─", ColMuted),
            StepState.Error   => ("✕", ColRed),
            _                 => ("○", ColMuted)
        };
        _stepTexts[i].Text      = text;
        _stepTexts[i].ForeColor = state switch
        {
            StepState.Done    => ColText,
            StepState.Running => ColYellow,
            StepState.Error   => ColRed,
            _                 => ColMuted
        };
    }

    private void SetProgress(int pct)
    {
        if (InvokeRequired) { Invoke(() => SetProgress(pct)); return; }
        _pnlProgressFill.Width    = (int)(_pnlProgressTrack.Width * pct / 100.0);
        _pnlProgressFill.BackColor = pct >= 100 ? ColGreen : ColAccent;
    }

    private void SetStatus(string msg)
    {
        if (InvokeRequired) { Invoke(() => SetStatus(msg)); return; }
        _lblStatus.Text = msg;
    }
}
