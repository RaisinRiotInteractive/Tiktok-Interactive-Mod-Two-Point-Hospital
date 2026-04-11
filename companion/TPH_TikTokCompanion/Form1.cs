using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using TikTokLiveSharp.Client;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Events.Objects;

namespace TPH_TikTokCompanion
{
    // ── Config model ──────────────────────────────────────────────────

    public class EventRule
    {
        public string Action         { get; set; } = "Nothing";
        public int    Amount         { get; set; } = 0;
        public int    LikeThreshold  { get; set; } = 1;   // only used by Like rule
    }

    public class GiftRule
    {
        public string GiftName { get; set; } = "";
        public string Action   { get; set; } = "AddMoney";
        public int    Amount   { get; set; } = 500;
    }

    public class AppConfig
    {
        public string       Username    { get; set; } = "";
        public EventRule    Follow      { get; set; } = new() { Action = "SpawnPatient", Amount = 0   };
        public EventRule    Like        { get; set; } = new() { Action = "AddMoney",     Amount = 10  };
        public EventRule    DefaultGift { get; set; } = new() { Action = "AddMoney",     Amount = 500 };
        public List<GiftRule> GiftRules { get; set; } = new()
        {
            new() { GiftName = "Rose",   Action = "AddMoney", Amount = 100  },
            new() { GiftName = "Galaxy", Action = "AddMoney", Amount = 5000 },
            new() { GiftName = "Lion",   Action = "AddMoney", Amount = 5000 },
        };
    }

    // ── Form ──────────────────────────────────────────────────────────

    public partial class Form1 : Form
    {
        private TikTokLiveClient? _client;
        private CancellationTokenSource? _cts;
        private bool _connected  = false;
        private bool _connecting = false;

        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tph_rules.json");

        private AppConfig _config = new();

        private static readonly string[] ActionNames = { "Spawn Patient", "Spawn Doctor", "Spawn Nurse", "Spawn Janitor", "Spawn Assistant", "Spawn Random", "Add Money", "Take Money", "Nothing" };
        private static readonly string[] ActionKeys  = { "SpawnPatient",  "SpawnDoctor",  "SpawnNurse",  "SpawnJanitor",  "SpawnAssistant",  "SpawnRandom",  "AddMoney",  "TakeMoney",  "Nothing" };

        private static readonly string[] SpawnableRoles = { "SpawnPatient", "SpawnDoctor", "SpawnNurse", "SpawnJanitor", "SpawnAssistant" };
        private static readonly Random _rng = new();

        // Like threshold options
        private static readonly string[] ThresholdNames  = { "Per like", "Every 10 likes", "Every 100 likes", "Every 1000 likes" };
        private static readonly int[]    ThresholdValues = { 1, 10, 100, 1000 };
        private int _likeAccumulator = 0;

        private static bool IsMoneyAction(string key) => key is "AddMoney" or "TakeMoney";
        private static bool IsSpawnAction(string key) => key.StartsWith("Spawn");
        private static string StaffRoleFromKey(string key) => key.StartsWith("SpawnStaff") ? key[5..]
            : key.StartsWith("Spawn") && key != "SpawnPatient" && key != "SpawnRandom" ? key[5..] : "";

        // ── Theme colours ─────────────────────────────────────────────
        static readonly Color ColBg      = Color.FromArgb(32,  34,  37);
        static readonly Color ColCard    = Color.FromArgb(47,  49,  54);
        static readonly Color ColInput   = Color.FromArgb(64,  68,  75);
        static readonly Color ColText    = Color.FromArgb(220, 221, 222);
        static readonly Color ColMuted   = Color.FromArgb(148, 155, 164);
        static readonly Color ColAccent  = Color.FromArgb(88,  101, 242);
        static readonly Color ColSuccess = Color.FromArgb(87,  242, 135);
        static readonly Color ColDanger  = Color.FromArgb(237, 66,  69);

        // ── Known TikTok gifts for autocomplete ───────────────────────
        static readonly string[] TikTokGifts =
        {
            "Rose", "TikTok", "Finger Heart", "Thumbs Up", "Heart", "Sun Cream",
            "Sunglasses Dude", "Drama Queen", "Panda", "Love Bang", "Ice Cream Cone",
            "Cowboy Hat", "Galaxy", "Lion", "Universe", "Perfume", "Donut", "Mic",
            "Camera", "Rainbow Puke", "Little Crown", "Box Car", "Confetti",
            "Blue Orca", "Fire", "Fireworks", "Football", "Medal", "Teamwork",
            "Corgi", "Knight", "Planet", "Rocket", "Interstellar", "Star",
            "Shooting Star", "Comet", "Sports Car", "Rainbow", "Cloud", "Cap",
            "Musical Note", "Money Bomb", "Castle", "Crown", "Balloon",
            "Cherry Blossom", "Cat Rabbit", "Santa", "Doughnut", "Soccer Ball",
            "Goat", "Thunder", "Pirate", "King Crown", "GG", "Dinosaur",
            "Butterfly", "Magic Wand", "Cheer Wine", "Hand Heart", "Flower",
            "Diamond", "Gold Diamond", "Clapping", "Happy Birthday", "Good Night",
            "Rosa", "Rabbit", "Beer", "Coffee", "Bubble Tea", "Boxing Gloves",
            "High Five", "Lover", "Potato", "Lollipop", "Concert", "Ferris Wheel",
            "Yacht", "Private Jet", "Motorcycle", "Butterfly Sword", "Lion Dance",
            "Love Explosion", "Falcon", "Phoenix", "Bald Eagle"
        };

        public Form1()
        {
            InitializeComponent();
            LoadConfig();
            PopulateRulesUI();
            ApplyTheme();
            UpdateUI();
        }

        // ── Dark title bar (Windows 10/11) ────────────────────────────
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int dark = 1;
            DwmSetWindowAttribute(Handle, 20, ref dark, sizeof(int)); // DWMWA_USE_IMMERSIVE_DARK_MODE
        }

        // ── Dark theme ────────────────────────────────────────────────

        private void ApplyTheme()
        {
            BackColor = ColBg;
            ForeColor = ColText;
            Font      = new System.Drawing.Font("Segoe UI", 9.5f);

            tabLive.BackColor  = ColBg;
            tabRules.BackColor = ColBg;

            tabMain.DrawMode  = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            tabMain.DrawItem += TabMain_DrawItem;
            tabMain.BackColor = ColBg;
            tabMain.Padding   = new System.Drawing.Point(14, 5);

            ApplyThemeToControls(Controls);
        }

        private void TabMain_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tab      = tabMain.TabPages[e.Index];
            bool selected = e.Index == tabMain.SelectedIndex;

            // Fill entire tab background (covers native rendering)
            Color bg = selected ? Color.FromArgb(54, 57, 63) : ColBg;
            e.Graphics.FillRectangle(new System.Drawing.SolidBrush(bg), e.Bounds);

            // Accent bar at bottom of selected tab
            if (selected)
            {
                e.Graphics.FillRectangle(
                    new System.Drawing.SolidBrush(ColAccent),
                    e.Bounds.Left, e.Bounds.Bottom - 2, e.Bounds.Width, 2);
            }

            // Tab text
            Color txt = selected ? ColText : ColMuted;
            var fmt = new System.Drawing.StringFormat {
                Alignment     = System.Drawing.StringAlignment.Center,
                LineAlignment = System.Drawing.StringAlignment.Center
            };
            var textRect = selected
                ? new System.Drawing.Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height - 2)
                : e.Bounds;
            e.Graphics.DrawString(tab.Text, Font, new System.Drawing.SolidBrush(txt), textRect, fmt);
        }

        private void ApplyThemeToControls(System.Windows.Forms.Control.ControlCollection controls)
        {
            foreach (System.Windows.Forms.Control c in controls)
            {
                switch (c)
                {
                    case System.Windows.Forms.Button btn:
                        btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                        btn.BackColor = Color.FromArgb(54, 57, 63);
                        btn.ForeColor = ColText;
                        btn.FlatAppearance.BorderColor        = Color.FromArgb(70, 73, 82);
                        btn.FlatAppearance.BorderSize         = 1;
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 68, 78);
                        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(44, 47, 56);
                        btn.Cursor = System.Windows.Forms.Cursors.Hand;
                        break;
                    case System.Windows.Forms.TextBox tb:
                        tb.BackColor   = ColInput;
                        tb.ForeColor   = ColText;
                        tb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                        break;
                    case System.Windows.Forms.ComboBox cb:
                        cb.BackColor = ColInput;
                        cb.ForeColor = ColText;
                        cb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                        break;
                    case System.Windows.Forms.NumericUpDown nud:
                        nud.BackColor   = ColInput;
                        nud.ForeColor   = ColText;
                        nud.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                        break;
                    case System.Windows.Forms.GroupBox gb:
                        gb.BackColor = ColCard;
                        // Hide native text (same colour as bg) — PaintGroupBox draws our own
                        gb.ForeColor = ColCard;
                        gb.Paint    += PaintGroupBox;
                        break;
                    case System.Windows.Forms.ListBox lb:
                        lb.BackColor   = Color.FromArgb(40, 42, 47);
                        lb.ForeColor   = ColText;
                        lb.BorderStyle = System.Windows.Forms.BorderStyle.None;
                        break;
                    case System.Windows.Forms.DataGridView dgv:
                        StyleDataGridView(dgv);
                        break;
                    case System.Windows.Forms.Label lbl:
                        lbl.BackColor = Color.Transparent;
                        lbl.ForeColor = ColText;
                        break;
                    case System.Windows.Forms.TabControl:
                    case System.Windows.Forms.TabPage:
                        c.BackColor = ColBg;
                        break;
                    default:
                        c.BackColor = ColBg;
                        c.ForeColor = ColText;
                        break;
                }

                if (c.Controls.Count > 0)
                    ApplyThemeToControls(c.Controls);
            }
        }

        // Custom GroupBox painter — erases native Win32 border and draws our own
        private void PaintGroupBox(object? sender, PaintEventArgs e)
        {
            if (sender is not System.Windows.Forms.GroupBox gb) return;
            var g = e.Graphics;

            // Fill entire client area to erase native border/text
            using var bgBrush = new System.Drawing.SolidBrush(ColCard);
            g.FillRectangle(bgBrush, new System.Drawing.Rectangle(0, 0, gb.Width, gb.Height));

            // Subtle border rectangle that starts halfway through the title area
            const int titleH = 13;
            var borderRect = new System.Drawing.Rectangle(0, titleH / 2, gb.Width - 1, gb.Height - titleH / 2 - 1);
            using var borderPen = new System.Drawing.Pen(Color.FromArgb(62, 65, 74));
            g.DrawRectangle(borderPen, borderRect);

            // Draw our styled title text
            if (!string.IsNullOrEmpty(gb.Text))
            {
                using var titleFont  = new System.Drawing.Font(gb.Font.FontFamily, gb.Font.Size, System.Drawing.FontStyle.Bold);
                using var titleBrush = new System.Drawing.SolidBrush(ColMuted);
                var measure = g.MeasureString(gb.Text, titleFont);
                // Blank out the border line behind the title text
                g.FillRectangle(bgBrush, 9, 0, (int)measure.Width + 6, titleH + 2);
                g.DrawString(gb.Text, titleFont, titleBrush, 12, 0);
            }
        }

        private static void StyleDataGridView(System.Windows.Forms.DataGridView dgv)
        {
            dgv.BackgroundColor                                  = Color.FromArgb(44, 47, 52);
            dgv.BorderStyle                                      = System.Windows.Forms.BorderStyle.None;
            dgv.GridColor                                        = Color.FromArgb(56, 59, 66);
            dgv.DefaultCellStyle.BackColor                       = Color.FromArgb(47, 49, 54);
            dgv.DefaultCellStyle.ForeColor                       = Color.FromArgb(220, 221, 222);
            dgv.DefaultCellStyle.SelectionBackColor              = Color.FromArgb(88, 101, 242);
            dgv.DefaultCellStyle.SelectionForeColor              = Color.White;
            dgv.DefaultCellStyle.Padding                         = new System.Windows.Forms.Padding(2, 0, 2, 0);
            dgv.AlternatingRowsDefaultCellStyle.BackColor        = Color.FromArgb(40, 43, 48);
            dgv.ColumnHeadersDefaultCellStyle.BackColor          = Color.FromArgb(36, 38, 43);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor          = Color.FromArgb(148, 155, 164);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(36, 38, 43);
            dgv.ColumnHeadersDefaultCellStyle.Font               = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            dgv.ColumnHeadersBorderStyle                         = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dgv.EnableHeadersVisualStyles                        = false;
            dgv.CellBorderStyle                                  = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.RowsDefaultCellStyle.BackColor                   = Color.FromArgb(47, 49, 54);
        }

        // ── Gift autocomplete ─────────────────────────────────────────

        private void dgvGifts_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvGifts.CurrentCell?.ColumnIndex == 0 && e.Control is TextBox tb)
            {
                tb.AutoCompleteMode   = AutoCompleteMode.SuggestAppend;
                tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
                var src = new AutoCompleteStringCollection();
                src.AddRange(TikTokGifts);
                tb.AutoCompleteCustomSource = src;
            }
            else if (e.Control is TextBox other)
            {
                other.AutoCompleteMode = AutoCompleteMode.None;
            }
        }

        // ── Config ────────────────────────────────────────────────────

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                    _config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new AppConfig();
            }
            catch { _config = new AppConfig(); }

            if (!string.IsNullOrEmpty(_config.Username))
                txtUsername.Text = _config.Username;
        }

        private void SaveConfig()
        {
            try
            {
                _config.Username = txtUsername.Text.Trim();

                _config.Follow.Action = ActionKeys[Math.Max(0, cmbFollowAction.SelectedIndex)];
                _config.Follow.Amount = (int)nudFollowAmount.Value;

                _config.Like.Action        = ActionKeys[Math.Max(0, cmbLikeAction.SelectedIndex)];
                _config.Like.Amount        = (int)nudLikeAmount.Value;
                _config.Like.LikeThreshold = ThresholdValues[Math.Max(0, cmbLikeThreshold.SelectedIndex)];
                _likeAccumulator = 0; // reset when rules change

                _config.DefaultGift.Action = ActionKeys[Math.Max(0, cmbDefaultGiftAction.SelectedIndex)];
                _config.DefaultGift.Amount = (int)nudDefaultGiftAmount.Value;

                _config.GiftRules.Clear();
                foreach (DataGridViewRow row in dgvGifts.Rows)
                {
                    if (row.IsNewRow) continue;
                    string name   = row.Cells[0].Value?.ToString() ?? "";
                    string action = row.Cells[1].Value?.ToString() ?? "Add Money";
                    int.TryParse(row.Cells[2].Value?.ToString(), out int amount);
                    if (!string.IsNullOrWhiteSpace(name))
                        _config.GiftRules.Add(new GiftRule
                        {
                            GiftName = name,
                            Action   = ActionKeyFromDisplayName(action),
                            Amount   = amount
                        });
                }

                File.WriteAllText(ConfigPath,
                    JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));
                Log("Rules saved.");
            }
            catch (Exception ex)
            {
                Log($"Save error: {ex.Message}");
            }
        }

        private void PopulateRulesUI()
        {
            // Populate combos
            cmbFollowAction.Items.Clear();
            cmbFollowAction.Items.AddRange(ActionNames);
            cmbLikeAction.Items.Clear();
            cmbLikeAction.Items.AddRange(ActionNames);
            cmbDefaultGiftAction.Items.Clear();
            cmbDefaultGiftAction.Items.AddRange(ActionNames);

            cmbFollowAction.SelectedIndex      = ActionIndex(_config.Follow.Action);
            nudFollowAmount.Value              = _config.Follow.Amount;

            cmbLikeAction.SelectedIndex        = ActionIndex(_config.Like.Action);
            nudLikeAmount.Value                = _config.Like.Amount;
            cmbLikeThreshold.Items.Clear();
            cmbLikeThreshold.Items.AddRange(ThresholdNames);
            cmbLikeThreshold.SelectedIndex     = ThresholdIndex(_config.Like.LikeThreshold);

            cmbDefaultGiftAction.SelectedIndex = ActionIndex(_config.DefaultGift.Action);
            nudDefaultGiftAmount.Value         = _config.DefaultGift.Amount;

            // Populate gift grid
            dgvGifts.Rows.Clear();
            foreach (var gr in _config.GiftRules)
                dgvGifts.Rows.Add(gr.GiftName, ActionDisplayName(gr.Action), gr.Amount);

            RefreshAmountVisibility();
        }

        private int ActionIndex(string key)
        {
            int i = Array.IndexOf(ActionKeys, key);
            return i >= 0 ? i : 3; // default "Nothing"
        }

        private int ThresholdIndex(int val)
        {
            int i = Array.IndexOf(ThresholdValues, val);
            return i >= 0 ? i : 0; // default "Per like"
        }

        private string ActionDisplayName(string key)
        {
            int i = Array.IndexOf(ActionKeys, key);
            return i >= 0 ? ActionNames[i] : ActionNames[3];
        }

        private string ActionKeyFromDisplayName(string display)
        {
            int i = Array.IndexOf(ActionNames, display);
            return i >= 0 ? ActionKeys[i] : ActionKeys[3];
        }

        private void RefreshAmountVisibility()
        {
            bool followMoney = cmbFollowAction.SelectedIndex >= 0 && IsMoneyAction(ActionKeys[cmbFollowAction.SelectedIndex]);
            lblFollowAmount.Visible = followMoney;
            nudFollowAmount.Visible = followMoney;

            bool likeMoney = cmbLikeAction.SelectedIndex >= 0 && IsMoneyAction(ActionKeys[cmbLikeAction.SelectedIndex]);
            lblLikeAmount.Visible = likeMoney;
            nudLikeAmount.Visible = likeMoney;

            bool defaultGiftMoney = cmbDefaultGiftAction.SelectedIndex >= 0 && IsMoneyAction(ActionKeys[cmbDefaultGiftAction.SelectedIndex]);
            lblDefaultGiftAmount.Visible = defaultGiftMoney;
            nudDefaultGiftAmount.Visible = defaultGiftMoney;
        }

        private void cmbFollowAction_SelectedIndexChanged(object sender, EventArgs e)      => RefreshAmountVisibility();
        private void cmbLikeAction_SelectedIndexChanged(object sender, EventArgs e)        => RefreshAmountVisibility();
        private void cmbDefaultGiftAction_SelectedIndexChanged(object sender, EventArgs e) => RefreshAmountVisibility();

        private void btnSaveRules_Click(object sender, EventArgs e) => SaveConfig();

        private void btnAddGift_Click(object sender, EventArgs e)
            => dgvGifts.Rows.Add("GiftName", "Add Money", 500);

        private void btnRemoveGift_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvGifts.SelectedRows)
                if (!row.IsNewRow) dgvGifts.Rows.Remove(row);
        }

        // ── TikTok events ─────────────────────────────────────────────

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            // Cancel an in-progress connection attempt
            if (_connecting) { await CancelConnectAsync(); return; }

            // Disconnect if already connected
            if (_connected)  { await DisconnectAsync();    return; }

            string username = txtUsername.Text.Trim();
            if (string.IsNullOrEmpty(username)) { Log("Enter a TikTok username first."); return; }

            _connecting = true;
            UpdateUI();
            Log($"Connecting to @{username}...");

            _config.Username = username;
            SaveConfig();

            try
            {
                _cts    = new CancellationTokenSource();
                _client = new TikTokLiveClient(username, timeout: null);

                _client.OnConnected += (c, ev) => Invoke(() =>
                {
                    _connecting = false;
                    _connected  = true;
                    Log($"Connected — @{username} is live!");
                    UpdateUI();
                });

                _client.OnDisconnected += (c, ev) => Invoke(() =>
                {
                    // If OnConnected never fired, the user wasn't live (or connection failed)
                    bool wasConnecting = _connecting;
                    _connecting = false;
                    _connected  = false;
                    Log(wasConnecting ? $"@{username} does not appear to be live." : "Disconnected.");
                    UpdateUI();
                });

                _client.OnFollow      += OnFollow;
                _client.OnLike        += OnLike;
                _client.OnGift        += OnGift;
                _client.OnChatMessage += OnChat;

                // Run in background; handle faults so we can surface "not live" errors
                var runTask = Task.Run(() => _client.RunAsync(_cts.Token));
                _ = runTask.ContinueWith(t =>
                {
                    if (!t.IsFaulted) return;
                    Invoke(() =>
                    {
                        bool wasConnecting = _connecting;
                        _connecting = false;
                        _connected  = false;
                        string msg = t.Exception?.InnerException?.Message ?? t.Exception?.Message ?? "Unknown error";
                        if (wasConnecting && IsNotLiveError(msg))
                            Log($"@{username} is not currently live.");
                        else
                            Log($"Connection error: {msg}");
                        UpdateUI();
                    });
                }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                _connecting = false;
                _connected  = false;
                Log($"Error: {ex.Message}");
                UpdateUI();
            }
        }

        private static bool IsNotLiveError(string msg)
        {
            var m = msg.ToLowerInvariant();
            return m.Contains("not live")     || m.Contains("offline")      ||
                   m.Contains("no live room") || m.Contains("not found")    ||
                   m.Contains("live stream")  || m.Contains("unavailable")  ||
                   m.Contains("no room")      || m.Contains("not streaming");
        }

        private async Task CancelConnectAsync()
        {
            _connecting = false;
            try { _cts?.Cancel(); if (_client != null) await _client.Stop(); } catch { }
            _connected = false;
            UpdateUI();
            Log("Connection cancelled.");
        }

        private async Task DisconnectAsync()
        {
            try { _cts?.Cancel(); if (_client != null) await _client.Stop(); } catch { }
            _connected  = false;
            _connecting = false;
            UpdateUI();
            Log("Disconnected.");
        }

        private void OnFollow(TikTokLiveClient c, Follow e)
        {
            string avatarUrl   = e.User.AvatarThumbnail?.Urls?.Count > 0 ? e.User.AvatarThumbnail.Urls[0] : "";
            string displayName = string.IsNullOrEmpty(e.User.NickName) ? e.User.UniqueId : e.User.NickName;
            Log($"Follow: @{e.User.UniqueId} ({displayName})  → {_config.Follow.Action}");
            Invoke(() => ApplyRule(_config.Follow, displayName, avatarUrl));
        }

        private void OnLike(TikTokLiveClient c, Like e)
        {
            var rule      = _config.Like;
            int threshold = rule.LikeThreshold > 0 ? rule.LikeThreshold : 1;
            string displayName = string.IsNullOrEmpty(e.Sender.NickName) ? e.Sender.UniqueId : e.Sender.NickName;
            string avatarUrl   = e.Sender.AvatarThumbnail?.Urls?.Count > 0 ? e.Sender.AvatarThumbnail.Urls[0] : "";

            _likeAccumulator += (int)e.Count;
            int triggers      = _likeAccumulator / threshold;
            _likeAccumulator %= threshold;

            if (triggers == 0)
            {
                Log($"Like ×{e.Count} from @{e.Sender.UniqueId}  ({_likeAccumulator}/{threshold} accumulated)");
                return;
            }

            bool isMoney = IsMoneyAction(rule.Action);
            int  amount  = isMoney ? triggers * rule.Amount : rule.Amount;
            Log($"Like ×{e.Count} from @{e.Sender.UniqueId}  → {triggers}× {rule.Action}" +
                (isMoney ? $" £{amount}" : "") +
                (_likeAccumulator > 0 ? $"  ({_likeAccumulator}/{threshold} carry)" : ""));

            Invoke(() => ApplyRule(new EventRule { Action = rule.Action, Amount = amount }, displayName, avatarUrl));
        }

        private void OnGift(TikTokLiveClient c, TikTokGift e)
        {
            string displayName = string.IsNullOrEmpty(e.Sender.NickName) ? e.Sender.UniqueId : e.Sender.NickName;
            string avatarUrl   = e.Sender.AvatarThumbnail?.Urls?.Count > 0 ? e.Sender.AvatarThumbnail.Urls[0] : "";
            EventRule? matched = null;
            foreach (var gr in _config.GiftRules)
            {
                if (e.Gift.Name.Contains(gr.GiftName, StringComparison.OrdinalIgnoreCase) ||
                    gr.GiftName.Contains(e.Gift.Name, StringComparison.OrdinalIgnoreCase))
                {
                    matched = new EventRule { Action = gr.Action, Amount = gr.Amount };
                    break;
                }
            }
            EventRule rule = matched ?? _config.DefaultGift;
            Log($"Gift: {e.Gift.Name} from @{e.Sender.UniqueId}  → {rule.Action}" +
                (rule.Action is "AddMoney" or "TakeMoney" ? $" £{rule.Amount}" : ""));
            Invoke(() => ApplyRule(rule, displayName, avatarUrl));
        }

        private void OnChat(TikTokLiveClient c, Chat e)
            => Log($"Chat [@{e.Sender.UniqueId}]: {e.Message}");

        private void ApplyRule(EventRule rule, string displayName, string avatarUrl)
            => _ = ApplyRuleAsync(rule, displayName, avatarUrl);

        private async Task ApplyRuleAsync(EventRule rule, string displayName, string avatarUrl)
        {
            string action = rule.Action;

            // Resolve random spawn to a concrete role before processing
            if (action == "SpawnRandom")
            {
                action = SpawnableRoles[_rng.Next(SpawnableRoles.Length)];
                Log($"[Random] Picked: {action}");
            }

            if (action == "SpawnPatient")
            {
                string filePath = await DownloadAvatarAsPngAsync(avatarUrl);
                SendCommand($"SPAWN:{displayName}|{filePath}");
            }
            else if (action is "SpawnDoctor" or "SpawnNurse" or "SpawnJanitor" or "SpawnAssistant")
            {
                string role     = StaffRoleFromKey(action);
                string filePath = await DownloadAvatarAsPngAsync(avatarUrl);
                SendCommand($"SPAWNSTAFF:{role}|{displayName}|{filePath}");
            }
            else if (action == "AddMoney")  SendCommand($"MONEY:{rule.Amount}");
            else if (action == "TakeMoney") SendCommand($"MONEY:{-rule.Amount}");
        }

        // Downloads any avatar URL and converts it to a temp PNG file using WIC.
        // WIC has a built-in WebP codec on Windows 10/11.
        // Returns the temp file path, or "" on failure.
        private static readonly HttpClient _http = new();

        private async Task<string> DownloadAvatarAsPngAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            try
            {
                byte[] data = await _http.GetByteArrayAsync(url);
                Log($"Avatar downloaded: {data.Length} bytes");

                // Decode with ImageSharp — pure managed, supports WebP natively
                string tmp = Path.Combine(Path.GetTempPath(), $"tph_avatar_{Guid.NewGuid()}.png");

                using var image = SixLabors.ImageSharp.Image.Load(data);
                await image.SaveAsPngAsync(tmp);

                Log($"Avatar saved: {image.Width}×{image.Height} → {tmp}");
                return tmp;
            }
            catch (Exception ex)
            {
                Log($"Avatar download failed: {ex.Message}");
                return "";
            }
        }

        // ── Named Pipe ────────────────────────────────────────────────

        private void SendCommand(string command)
        {
            Task.Run(() =>
            {
                try
                {
                    using var pipe = new NamedPipeClientStream(".", "TPHTikTokMod", PipeDirection.Out);
                    pipe.Connect(500);
                    using var writer = new StreamWriter(pipe);
                    writer.WriteLine(command);
                    writer.Flush();
                }
                catch { /* game not running */ }
            });
        }

        // ── Test Buttons ──────────────────────────────────────────────

        private void btnTestFollow_Click(object sender, EventArgs e)
        {
            Log("[Test] Simulated Follow");
            ApplyRule(_config.Follow, "TestViewer", "");
        }

        private void btnTestLike_Click(object sender, EventArgs e)
        {
            var rule      = _config.Like;
            int threshold = rule.LikeThreshold > 0 ? rule.LikeThreshold : 1;
            // Simulate enough likes to guarantee at least one trigger
            int simCount  = Math.Max(50, threshold);
            int triggers  = simCount / threshold;
            bool isMoney  = IsMoneyAction(rule.Action);
            int  amount   = isMoney ? triggers * rule.Amount : rule.Amount;
            Log($"[Test] Simulated Like ×{simCount}  → {triggers}× {rule.Action}" + (isMoney ? $" £{amount}" : ""));
            ApplyRule(new EventRule { Action = rule.Action, Amount = amount }, "TestViewer", "");
        }

        private void btnTestGift_Click(object sender, EventArgs e)
        {
            Log("[Test] Simulated default Gift");
            ApplyRule(_config.DefaultGift, "TestViewer", "");
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            SendCommand("INIT");
            Log("[Test] Sent INIT");
        }

        private void btnSpawnDoctor_Click(object sender, EventArgs e)
        {
            SendCommand("SPAWNSTAFF:Doctor|TestDoctor|");
            Log("[Test] Spawning Doctor");
        }

        private void btnSpawnNurse_Click(object sender, EventArgs e)
        {
            SendCommand("SPAWNSTAFF:Nurse|TestNurse|");
            Log("[Test] Spawning Nurse");
        }

        private void btnSpawnJanitor_Click(object sender, EventArgs e)
        {
            SendCommand("SPAWNSTAFF:Janitor|TestJanitor|");
            Log("[Test] Spawning Janitor");
        }

        private void btnSpawnAssistant_Click(object sender, EventArgs e)
        {
            SendCommand("SPAWNSTAFF:Assistant|TestAssistant|");
            Log("[Test] Spawning Assistant");
        }

        // ── Helpers ───────────────────────────────────────────────────

        private static readonly string LogFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tph_companion.log");

        private void Log(string message)
        {
            if (InvokeRequired) { Invoke(() => Log(message)); return; }
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            lstLog.Items.Insert(0, line);
            if (lstLog.Items.Count > 200) lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
            try { File.AppendAllText(LogFilePath, line + Environment.NewLine); } catch { }
        }

        private void btnCopyLog_Click(object sender, EventArgs e)
        {
            var lines = new System.Text.StringBuilder();
            for (int i = lstLog.Items.Count - 1; i >= 0; i--)
                lines.AppendLine(lstLog.Items[i]?.ToString());
            if (lines.Length > 0)
            {
                Clipboard.SetText(lines.ToString());
                MessageBox.Show("Log copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateUI()
        {
            if (InvokeRequired) { Invoke(UpdateUI); return; }

            if (_connecting)
            {
                btnConnect.Text      = "Cancel";
                btnConnect.BackColor = Color.FromArgb(90, 75, 15);
                btnConnect.ForeColor = Color.White;
                btnConnect.FlatAppearance.BorderColor        = Color.FromArgb(180, 150, 30);
                btnConnect.FlatAppearance.MouseOverBackColor = Color.FromArgb(120, 100, 20);
                btnConnect.FlatAppearance.MouseDownBackColor = Color.FromArgb(70,  60, 10);
                lblStatus.Text      = $"◌ Connecting to @{txtUsername.Text.Trim()}...";
                lblStatus.ForeColor = Color.FromArgb(220, 180, 50);
            }
            else if (_connected)
            {
                btnConnect.Text      = "Disconnect";
                btnConnect.BackColor = ColDanger;
                btnConnect.ForeColor = Color.White;
                btnConnect.FlatAppearance.BorderColor        = ColDanger;
                btnConnect.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 50, 52);
                btnConnect.FlatAppearance.MouseDownBackColor = Color.FromArgb(170, 40, 42);
                lblStatus.Text      = $"● Live: @{txtUsername.Text.Trim()}";
                lblStatus.ForeColor = ColSuccess;
            }
            else
            {
                btnConnect.Text      = "Connect";
                btnConnect.BackColor = ColAccent;
                btnConnect.ForeColor = Color.White;
                btnConnect.FlatAppearance.BorderColor        = ColAccent;
                btnConnect.FlatAppearance.MouseOverBackColor = Color.FromArgb(110, 120, 255);
                btnConnect.FlatAppearance.MouseDownBackColor = Color.FromArgb(70,   85, 210);
                lblStatus.Text      = "○ Not connected";
                lblStatus.ForeColor = ColMuted;
            }

            btnConnect.Cursor  = System.Windows.Forms.Cursors.Hand;
            btnConnect.Enabled = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();
            base.OnFormClosing(e);
        }
    }
}
