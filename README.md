# TikTok Interactive Mode — Two Point Hospital

A BepInEx mod + companion app that connects your TikTok LIVE stream to Two Point Hospital. Followers spawn as patients or staff, likes add money, gifts trigger custom events — all configurable through the companion app without touching any code.

---

## Features

| TikTok Event | What happens in-game |
|---|---|
| **Follow** | Spawns a character (patient, doctor, nurse, janitor, assistant, or random) named after the follower, with their profile picture floating above them |
| **Like** | Adds money — configurable per like, every 10, every 100, or every 1000 likes |
| **Gift** | Configurable per gift name with autocomplete for all known TikTok gifts |
| **Chat** | Logged to the companion app event log |

**Other highlights:**
- Profile pictures appear as floating billboards above spawned characters
- Avatars survive game saves and reloads
- In-game overlay (top-right) shows connection status and a button to open the companion app — press **F9** to hide/show it
- Companion app: dark themed, configurable rules per event, cancel-able connection that checks the user is actually live before reporting connected

---

## Requirements

- **Two Point Hospital** (Steam — tested on latest version)
- **BepInEx 5.x (x64)** installed in the game folder — [download here](https://github.com/BepInEx/BepInEx/releases)
- **Windows 10 or 11** (companion app is Windows-only)
- **.NET 9 Runtime** for the companion app (bundled in the self-contained release)
- **.NET Framework 4.7.2 SDK** only if building from source

---

## Installation (players)

1. **Install BepInEx 5.x (x64)** into your Two Point Hospital folder if you haven't already.

2. **Download the latest release** from the [Releases](../../releases) page.

3. **Copy `TPH_TikTokMod.dll`** into:
   ```
   [TPH install folder]\BepInEx\plugins\
   ```

4. **Extract the companion app** folder (`TPH_TikTokCompanion\`) into your TPH install folder so it sits alongside `BepInEx\`:
   ```
   [TPH install folder]\
   ├── BepInEx\
   │   └── plugins\
   │       └── TPH_TikTokMod.dll   ← mod goes here
   ├── TPH_TikTokCompanion\
   │   └── TPH_TikTokCompanion.exe ← companion app goes here
   └── TwoPointHospital.exe
   ```

5. **Start Two Point Hospital**, then **run `TPH_TikTokCompanion.exe`**.

6. In the companion app:
   - Enter your TikTok username (without @)
   - Configure your event rules on the **Rules** tab
   - Click **Connect** — it will only report Connected once your live stream is detected

7. The **in-game overlay** (top-right corner) will show a green stripe when the companion is active. Click **Open Companion App** from there or press **F9** to toggle the overlay.

---

## Companion App — Rules

Open the **Rules** tab to configure what happens for each event type.

### Follow
Choose an action (spawn patient, doctor, nurse, janitor, assistant, random, add money, or nothing).

### Likes
Choose an action and a **trigger threshold**:
- **Per like** — fires for every like in each batch
- **Every 10 / 100 / 1000 likes** — accumulates likes across events and fires once the threshold is crossed; leftover likes carry forward

### Gifts
Add rows for specific gift names (with autocomplete for ~80 known TikTok gifts) and set per-gift actions. A **Default gift** action covers any gift not in the list.

Click **Save Rules** to persist your config to `tph_rules.json` next to the companion app.

---

## Building from Source

### 1. Clone the repo

```bash
git clone https://github.com/RaisinRiotInteractive/Tiktok-Interactive-Mode-Two-Point-Hospital.git
cd Tiktok-Interactive-Mode-Two-Point-Hospital
```

### 2. Set your game path

```bash
copy GameDirectory.props.example GameDirectory.props
```

Open `GameDirectory.props` and set `<GameDirectory>` to your TPH installation folder:

```xml
<GameDirectory>D:\SteamLibrary\steamapps\common\TPH</GameDirectory>
```

BepInEx must already be installed in that folder (the build references `BepInEx\core\BepInEx.dll` from there).

### 3. Build the mod

```bash
dotnet build TPH_TikTokMod.csproj -c Release
```

Output: `bin\Release\net472\TPH_TikTokMod.dll` — copy this to `[TPH]\BepInEx\plugins\`.

### 4. Build the companion app

```bash
cd companion\TPH_TikTokCompanion
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ..\..\publish\companion
```

Output: `publish\companion\TPH_TikTokCompanion.exe` — copy the whole folder to `[TPH]\TPH_TikTokCompanion\`.

---

## Project Structure

```
├── src/
│   ├── TikTokPlugin.cs         Main BepInEx plugin — pipe listener, in-game overlay, texture loading
│   ├── GameInterface.cs        Game interaction via reflection — spawn patients/staff, money, avatars
│   └── Patches/
│       └── HospitalPatch.cs    Harmony patch on Level.RestoreFromSave — re-applies avatars on load
├── companion/
│   └── TPH_TikTokCompanion/
│       ├── Form1.cs            Companion app logic — TikTok connection, rules engine, pipe commands
│       └── Form1.Designer.cs   WinForms layout
├── TPH_TikTokMod.csproj        Mod project (net472, BepInEx)
├── GameDirectory.props.example Copy → GameDirectory.props and set your install path
└── NuGet.Config                Adds BepInEx NuGet feed
```

---

## How it works

The mod and companion communicate over a **Windows named pipe** (`TPHTikTokMod`):

```
TikTok LIVE → Companion App → Named Pipe → BepInEx Mod → Unity Game
```

The companion app connects to TikTok LIVE using [TikTokLive-Sharp](https://github.com/frankvHoof93/TikTokLiveSharp), translates events into commands, and sends them through the pipe. The mod receives commands on Unity's main thread and calls into the game via reflection (no game DLLs are redistributed — the mod reads the game's types at runtime).

Avatar images are downloaded by the companion, converted to PNG, and passed to the mod as file paths. The mod loads them as Unity textures and attaches them as world-space billboards above each character. Avatars are stored in `BepInEx\plugins\TikTokAvatars\` so they persist across saves.

---

## Disclaimer

This mod uses an unofficial TikTok Live connection library and accesses Two Point Hospital internals via reflection. It may break after game or TikTok updates. It is not affiliated with Two Point Studios, SEGA, or TikTok.
