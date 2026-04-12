using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace TPH_TikTokMod
{
    [BepInPlugin("com.user.tph.tiktok", "TPH TikTok Live Integration", "1.1.0")]
    public class TikTokPlugin : BaseUnityPlugin
    {
        public static TikTokPlugin Instance { get; private set; }

        private CancellationTokenSource _cts;

        // ── Overlay GUI ───────────────────────────────────────────────
        private DateTime   _lastCommandTime = DateTime.MinValue;
        private bool       _guiVisible      = true;
        private bool       _guiReady        = false;
        private GUIStyle   _stylePanel;
        private GUIStyle   _styleTitle;
        private GUIStyle   _styleStatus;
        private GUIStyle   _styleButton;
        private GUIStyle   _styleHideBtn;
        private GUIStyle   _styleShowTab;
        private Texture2D  _texPanel;
        private Texture2D  _texButton;
        private Texture2D  _texButtonHover;
        private Texture2D  _texSeparator;
        private Texture2D  _texHideBtn;
        private Texture2D  _texHideBtnHover;
        private Texture2D  _texShowTab;
        private Texture2D  _texShowTabHover;
        private Texture2D  _texStripeGreen;
        private Texture2D  _texStripeGrey;

        private const KeyCode ToggleKey = KeyCode.F9;

        private bool CompanionActive => (DateTime.Now - _lastCommandTime).TotalSeconds < 30;

        void Awake()
        {
            Debug.Log("[TikTokMod] Awake() start");
            Instance = this;

            try
            {
                var harmony = new Harmony("com.user.tph.tiktok");
                harmony.PatchAll();
                Debug.Log("[TikTokMod] Harmony patches applied");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TikTokMod] Harmony error: {ex.Message}");
            }

            _cts = new CancellationTokenSource();
            Task.Run(() => ListenForCommands(_cts.Token));

            Logger.LogInfo("TPH TikTok Mod Loaded! Listening for companion app commands.");
            Debug.Log("[TikTokMod] Awake() complete");
        }

        private async Task ListenForCommands(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var pipe = new NamedPipeServerStream("TPHTikTokMod", PipeDirection.In, 1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    await pipe.WaitForConnectionAsync(token);

                    using var reader = new StreamReader(pipe);
                    string line = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(line))
                    {
                        Debug.Log($"[TikTokMod] Command received: {line}");
                        UnityMainThread(() => ProcessCommand(line));
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[TikTokMod] Pipe error: {ex.Message}");
                    await Task.Delay(1000, token).ConfigureAwait(false);
                }
            }
        }

        private void ProcessCommand(string command)
        {
            _lastCommandTime = DateTime.Now;
            try
            {
                if (command.StartsWith("SPAWN:"))
                {
                    // Format: SPAWN:<displayName>|<avatarUrl>
                    string payload = command.Substring(6);
                    string[] parts = payload.Split('|');
                    string displayName = parts.Length > 0 && !string.IsNullOrEmpty(parts[0]) ? parts[0] : "TikTok Fan";
                    string avatarUrl   = parts.Length > 1 ? parts[1] : "";
                    Debug.Log($"[TikTokMod] Spawning patient: {displayName} | avatar: '{avatarUrl}'");
                    GameInterface.SpawnFollowerPatient(displayName, avatarUrl, this);
                }
                else if (command.StartsWith("MONEY:"))
                {
                    if (int.TryParse(command.Substring(6), out int amount) && amount != 0)
                    {
                        Debug.Log($"[TikTokMod] Money change: {amount}");
                        GameInterface.AddMoney(amount);
                    }
                }
                else if (command == "INIT")
                {
                    Debug.Log("[TikTokMod] Re-initialising GameInterface");
                    GameInterface.Initialise();
                }
                else if (command.StartsWith("SPAWNSTAFF:"))
                {
                    string payload = command.Substring(11);
                    string[] parts = payload.Split(new char[] { '|' }, 3);
                    string role        = parts.Length > 0 ? parts[0] : "Doctor";
                    string displayName = parts.Length > 1 ? parts[1] : "TikTok Fan";
                    string avatarPath  = parts.Length > 2 ? parts[2] : "";
                    Debug.Log($"[TikTokMod] Spawning staff: {role} '{displayName}' | avatar: '{avatarPath}'");
                    GameInterface.SpawnStaffMember(role, displayName, avatarPath, this);
                }
                // ── Legacy commands (backward compat) ─────────────────
                else if (command.StartsWith("FOLLOW:"))
                {
                    string payload    = command.Substring(7);
                    string[] parts    = payload.Split('|');
                    string uniqueId   = parts.Length > 0 ? parts[0] : payload;
                    string displayName = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : uniqueId;
                    string avatarUrl  = parts.Length > 2 ? parts[2] : "";
                    Debug.Log($"[TikTokMod] (legacy) Spawning patient for {displayName}");
                    GameInterface.SpawnFollowerPatient(displayName, avatarUrl, this);
                }
                else if (command.StartsWith("LIKE:"))
                {
                    if (long.TryParse(command.Substring(5), out long count))
                        GameInterface.AddMoney((int)(count * 10));
                }
                else if (command.StartsWith("GIFT:"))
                {
                    string giftName = command.Substring(5);
                    if (giftName.Contains("Rose"))        GameInterface.AddMoney(100);
                    else if (giftName.Contains("Galaxy") || giftName.Contains("Lion")) GameInterface.AddMoney(5000);
                    else                                  GameInterface.AddMoney(500);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TikTokMod] ProcessCommand error: {ex.Message}");
            }
        }

        // Load a texture from a temp file — deletes the file after loading.
        public void LoadTextureFromFile(string path, Action<Texture2D> callback)
            => StartCoroutine(LoadFileCoroutine(path, callback, deleteAfter: true));

        // Load a texture from a persistent file — does NOT delete the file.
        public void LoadPersistentTexture(string path, Action<Texture2D> callback)
            => StartCoroutine(LoadFileCoroutine(path, callback, deleteAfter: false));

        private IEnumerator LoadFileCoroutine(string path, Action<Texture2D> callback, bool deleteAfter = true)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith("http"))
            {
                Debug.LogWarning("[TikTokMod] Avatar skipped — companion app needs updating.");
                yield break;
            }

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[TikTokMod] Avatar file not found: {path}");
                yield break;
            }

            byte[] data;
            try   { data = File.ReadAllBytes(path); }
            catch { Debug.LogWarning($"[TikTokMod] Could not read avatar file: {path}"); yield break; }

            if (deleteAfter) { try { File.Delete(path); } catch { } }

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            // LoadImage handles PNG and JPEG; moved to ImageConversion in Unity 2019+
            bool loaded = false;
            var directLoad = typeof(Texture2D).GetMethod("LoadImage",
                BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(byte[]) }, null);
            if (directLoad != null)
                loaded = (bool)directLoad.Invoke(tex, new object[] { data });

            if (!loaded)
            {
                var imgConv = Type.GetType("UnityEngine.ImageConversion, UnityEngine.CoreModule")
                           ?? Type.GetType("UnityEngine.ImageConversion, UnityEngine");
                var extLoad = imgConv?.GetMethod("LoadImage",
                    BindingFlags.Static | BindingFlags.Public, null,
                    new[] { typeof(Texture2D), typeof(byte[]) }, null);
                if (extLoad != null)
                    loaded = (bool)extLoad.Invoke(null, new object[] { tex, data });
            }

            if (loaded)
            {
                Debug.Log($"[TikTokMod] Avatar loaded from file ({tex.width}×{tex.height})");
                callback(tex);
            }
            else
            {
                Debug.LogWarning($"[TikTokMod] Avatar file decode failed: {path}");
                UnityEngine.Object.Destroy(tex);
            }
        }

        // Run an action on Unity's main thread via Update()
        private static readonly System.Collections.Concurrent.ConcurrentQueue<Action> _queue
            = new System.Collections.Concurrent.ConcurrentQueue<Action>();

        public static void UnityMainThread(Action action) => _queue.Enqueue(action);

        void Update()
        {
            while (_queue.TryDequeue(out var action))
                action.Invoke();

            if (Input.GetKeyDown(ToggleKey))
                _guiVisible = !_guiVisible;
        }

        void OnDestroy()
        {
            _cts?.Cancel();
            if (_texPanel        != null) Destroy(_texPanel);
            if (_texButton       != null) Destroy(_texButton);
            if (_texButtonHover  != null) Destroy(_texButtonHover);
            if (_texSeparator    != null) Destroy(_texSeparator);
            if (_texHideBtn      != null) Destroy(_texHideBtn);
            if (_texHideBtnHover != null) Destroy(_texHideBtnHover);
            if (_texShowTab      != null) Destroy(_texShowTab);
            if (_texShowTabHover != null) Destroy(_texShowTabHover);
            if (_texStripeGreen  != null) Destroy(_texStripeGreen);
            if (_texStripeGrey   != null) Destroy(_texStripeGrey);
        }

        // ── Overlay GUI ───────────────────────────────────────────────

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = col;
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        private void InitOverlayGUI()
        {
            _texPanel        = MakeTex(4, 4, new Color(0.12f, 0.13f, 0.15f, 0.92f));
            _texButton       = MakeTex(4, 4, new Color(0.34f, 0.40f, 0.95f, 1f));
            _texButtonHover  = MakeTex(4, 4, new Color(0.45f, 0.50f, 1.00f, 1f));
            _texSeparator    = MakeTex(1, 1, new Color(0.27f, 0.28f, 0.31f, 1f));
            _texHideBtn      = MakeTex(4, 4, new Color(0.22f, 0.23f, 0.26f, 0.85f));
            _texHideBtnHover = MakeTex(4, 4, new Color(0.75f, 0.27f, 0.28f, 1f));
            _texShowTab      = MakeTex(4, 4, new Color(0.12f, 0.13f, 0.15f, 0.88f));
            _texShowTabHover = MakeTex(4, 4, new Color(0.22f, 0.24f, 0.28f, 1f));
            _texStripeGreen  = MakeTex(1, 1, new Color(0.20f, 0.78f, 0.38f, 1f));
            _texStripeGrey   = MakeTex(1, 1, new Color(0.40f, 0.42f, 0.46f, 1f));

            _stylePanel = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 8, 10) };
            _stylePanel.normal.background = _texPanel;

            _styleTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _styleTitle.normal.textColor = new Color(0.86f, 0.87f, 0.87f, 1f);

            _styleStatus = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                alignment = TextAnchor.MiddleCenter
            };

            _styleButton = new GUIStyle(GUI.skin.button)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _styleButton.normal.background  = _texButton;
            _styleButton.hover.background   = _texButtonHover;
            _styleButton.active.background  = _texButton;
            _styleButton.normal.textColor   = Color.white;
            _styleButton.hover.textColor    = Color.white;
            _styleButton.active.textColor   = Color.white;
            _styleButton.border             = new RectOffset(4, 4, 4, 4);

            // Small × close button in title bar
            _styleHideBtn = new GUIStyle(GUI.skin.button)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _styleHideBtn.normal.background  = _texHideBtn;
            _styleHideBtn.hover.background   = _texHideBtnHover;
            _styleHideBtn.active.background  = _texHideBtnHover;
            _styleHideBtn.normal.textColor   = new Color(0.70f, 0.72f, 0.75f, 1f);
            _styleHideBtn.hover.textColor    = Color.white;
            _styleHideBtn.active.textColor   = Color.white;
            _styleHideBtn.border             = new RectOffset(2, 2, 2, 2);

            // Collapsed show-tab button
            _styleShowTab = new GUIStyle(GUI.skin.button)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _styleShowTab.normal.background  = _texShowTab;
            _styleShowTab.hover.background   = _texShowTabHover;
            _styleShowTab.active.background  = _texShowTab;
            _styleShowTab.normal.textColor   = new Color(0.70f, 0.72f, 0.75f, 1f);
            _styleShowTab.hover.textColor    = Color.white;
            _styleShowTab.active.textColor   = Color.white;
            _styleShowTab.border             = new RectOffset(4, 4, 4, 4);

            _guiReady = true;
        }

        void OnGUI()
        {
            if (!_guiReady) InitOverlayGUI();

            const float W   = 214f;
            const float TAB_W = 120f;
            const float TAB_H = 24f;
            float tabX = Screen.width - TAB_W - 12;
            float tabY = 12;

            bool active = CompanionActive;

            if (!_guiVisible)
            {
                // Collapsed tab — colour indicates connection state
                _styleShowTab.normal.textColor = active
                    ? new Color(0.34f, 0.95f, 0.53f, 1f)
                    : new Color(0.70f, 0.72f, 0.75f, 1f);
                string tabLabel = (active ? "● " : "○ ") + "TikTok Mod  F9";
                if (GUI.Button(new Rect(tabX, tabY, TAB_W, TAB_H), tabLabel, _styleShowTab))
                    _guiVisible = true;
                return;
            }

            // ── Expanded panel ────────────────────────────────────────
            const float H      = 108f;
            const float HIDE_W = 22f;
            const float HIDE_H = 18f;
            float x = Screen.width - W - 12;
            float y = 12;

            GUI.Box(new Rect(x, y, W, H), GUIContent.none, _stylePanel);

            // Coloured stripe across the top of the panel (green = connected, grey = idle)
            GUI.DrawTexture(new Rect(x, y, W, 3), active ? _texStripeGreen : _texStripeGrey);

            // Title (leave room on the right for the × button)
            GUI.Label(new Rect(x, y + 5, W - HIDE_W - 6, 22), "TikTok Live Mod", _styleTitle);

            // × hide button — top-right corner of panel
            if (GUI.Button(new Rect(x + W - HIDE_W - 6, y + 5, HIDE_W, HIDE_H), "×", _styleHideBtn))
                _guiVisible = false;

            // Separator
            GUI.DrawTexture(new Rect(x + 10, y + 29, W - 20, 1), _texSeparator);

            // Status text
            _styleStatus.normal.textColor = active
                ? new Color(0.34f, 0.95f, 0.53f, 1f)
                : new Color(0.58f, 0.61f, 0.64f, 1f);
            GUI.Label(new Rect(x, y + 34, W, 20),
                active ? "● Companion Connected" : "○ Companion Idle", _styleStatus);

            // Open companion button
            if (GUI.Button(new Rect(x + 10, y + 60, W - 20, 28), "Open Companion App", _styleButton))
                OpenCompanionApp();

            // F9 hint
            var hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 9, alignment = TextAnchor.MiddleCenter };
            hintStyle.normal.textColor = new Color(0.40f, 0.42f, 0.46f, 1f);
            GUI.Label(new Rect(x, y + H - 14, W, 12), "F9 to hide", hintStyle);
        }

        private void OpenCompanionApp()
        {
            try
            {
                string gameRoot    = System.IO.Path.GetDirectoryName(Application.dataPath);
                string exePath     = System.IO.Path.Combine(gameRoot, "TPH_TikTokCompanion", "TPH_TikTokCompanion.exe");

                if (!System.IO.File.Exists(exePath))
                {
                    Debug.LogWarning($"[TikTokMod] Companion app not found at: {exePath}");
                    return;
                }

                var psi = new System.Diagnostics.ProcessStartInfo(exePath)
                {
                    UseShellExecute  = true,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(exePath)
                };
                System.Diagnostics.Process.Start(psi);
                Debug.Log("[TikTokMod] Companion app launched.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TikTokMod] Could not open companion app: {ex.Message}");
            }
        }
    }
}
