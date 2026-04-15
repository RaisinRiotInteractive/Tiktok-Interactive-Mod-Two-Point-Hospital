using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TPH_TikTokMod
{
    public static class GameInterface
    {
        private static object _app;
        private static object _level;
        private static object _financeManager;
        private static object _characterManager;
        private static bool _initialised = false;

        // ── Orphan tracking ───────────────────────────────────────────────
        // Characters spawned by this mod; cleaned up if the game de-lists them.
        private static readonly Dictionary<object, string> _trackedChars
            = new Dictionary<object, string>(); // character → display name

        // ── Avatar persistence ────────────────────────────────────────────
        // Keyed by character ID string → (displayName, charType)
        private static readonly Dictionary<string, (string name, string type)> _avatarMap
            = new Dictionary<string, (string, string)>();
        private static string _avatarFolder;

        private static string GetAvatarFolder()
        {
            if (_avatarFolder != null) return _avatarFolder;
            string pluginDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".";
            _avatarFolder = Path.Combine(pluginDir, "TikTokAvatars");
            try { Directory.CreateDirectory(_avatarFolder); } catch { }
            return _avatarFolder;
        }

        private static string GetCharacterId(object character)
        {
            try
            {
                return character.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(p => p.Name == "ID")
                    ?.GetValue(character)?.ToString();
            }
            catch { return null; }
        }

        // Copy avatar to persistent folder and record mapping.
        // Returns the persistent path (or empty if no avatar).
        private static string PersistAvatar(object character, string tempPath, string displayName, string charType)
        {
            if (string.IsNullOrEmpty(tempPath)) return "";
            string charId = GetCharacterId(character);
            if (string.IsNullOrEmpty(charId)) return tempPath;

            string folder      = GetAvatarFolder();
            string persistPath = Path.Combine(folder, $"{charId}.png");
            try
            {
                File.Copy(tempPath, persistPath, overwrite: true);
                Debug.Log($"[TikTokMod] Avatar persisted → {persistPath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TikTokMod] Failed to persist avatar: {ex.Message}");
                return tempPath;
            }

            _avatarMap[charId] = (displayName, charType);
            SaveAvatarMapping();
            return persistPath;
        }

        private static void SaveAvatarMapping()
        {
            try
            {
                string path = Path.Combine(GetAvatarFolder(), "mapping.txt");
                File.WriteAllLines(path, _avatarMap.Select(kv => $"{kv.Key}|{kv.Value.name}|{kv.Value.type}"));
            }
            catch (Exception ex) { Debug.LogWarning($"[TikTokMod] SaveAvatarMapping: {ex.Message}"); }
        }

        private static void LoadAvatarMapping()
        {
            try
            {
                string path = Path.Combine(GetAvatarFolder(), "mapping.txt");
                if (!File.Exists(path)) return;
                _avatarMap.Clear();
                foreach (var line in File.ReadAllLines(path))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 3) _avatarMap[parts[0]] = (parts[1], parts[2]);
                }
                Debug.Log($"[TikTokMod] Loaded {_avatarMap.Count} avatar mapping(s)");
            }
            catch (Exception ex) { Debug.LogWarning($"[TikTokMod] LoadAvatarMapping: {ex.Message}"); }
        }

        public static void ReapplyAllAvatars(TikTokPlugin plugin)
        {
            EnsureValid();
            if (_characterManager == null) return;
            LoadAvatarMapping();
            if (_avatarMap.Count == 0) return;
            plugin.StartCoroutine(ReapplyAvatarsCoroutine(plugin));
        }

        private static IEnumerator ReapplyAvatarsCoroutine(TikTokPlugin plugin)
        {
            yield return new WaitForSeconds(2f); // let characters fully initialise

            string folder = GetAvatarFolder();

            // Build id → character lookup from both patients and staff
            var lookup = new Dictionary<string, object>();
            foreach (var p in GetAllPatients(out _))
            {
                string id = GetCharacterId(p);
                if (id != null) lookup[id] = p;
            }
            foreach (var s in GetAllStaff())
            {
                string id = GetCharacterId(s);
                if (id != null) lookup[id] = s;
            }

            Debug.Log($"[TikTokMod] ReapplyAvatars: {lookup.Count} chars found, {_avatarMap.Count} entries");

            foreach (var kv in _avatarMap)
            {
                string charId      = kv.Key;
                string displayName = kv.Value.name;
                string avatarFile  = Path.Combine(folder, $"{charId}.png");

                if (!lookup.TryGetValue(charId, out object character)) continue;
                if (!File.Exists(avatarFile)) continue;

                object capturedChar = character;
                string capturedName = displayName;
                plugin.LoadPersistentTexture(avatarFile, tex =>
                    AttachAvatarBillboard(capturedChar, tex, capturedName));
            }
        }

        // Destroy Unity GameObjects for mod-spawned characters that the game has
        // de-listed (AI failure, bad-state cleanup, etc.) so they stop appearing
        // as visually stuck ghosts.
        public static void CleanupOrphanedCharacters()
        {
            if (!_initialised || _trackedChars.Count == 0) return;

            var patients = GetAllPatients(out _);
            var staff    = GetAllStaff();
            var active   = new HashSet<object>(patients);
            foreach (var s in staff) active.Add(s);

            var orphans = new List<object>();
            foreach (var kv in _trackedChars)
                if (!active.Contains(kv.Key)) orphans.Add(kv.Key);

            foreach (var orphan in orphans)
            {
                try
                {
                    var go = GetCharacterGameObject(orphan);
                    if (go != null && go != null)
                    {
                        Debug.Log($"[TikTokMod] Destroying orphaned character GameObject: '{_trackedChars[orphan]}'");
                        UnityEngine.Object.Destroy(go);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[TikTokMod] Orphan cleanup error for '{_trackedChars[orphan]}': {ex.Message}");
                }
                _trackedChars.Remove(orphan);
            }

            if (orphans.Count > 0)
                Debug.Log($"[TikTokMod] Cleaned up {orphans.Count} orphaned character(s).");
        }

        private static GameObject GetCharacterGameObject(object character)
        {
            try
            {
                var ct = character.GetType();
                var go = ct.GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public)
                           ?.GetValue(character) as GameObject;
                if (go != null) return go;

                var t = ct.GetProperty("Transform", BindingFlags.Instance | BindingFlags.Public)
                          ?.GetValue(character) as Transform;
                if (t != null) return t.gameObject;

                return (character as Component)?.gameObject;
            }
            catch { return null; }
        }

        // Always re-initialise before any spawn or money call.
        // Caching _level/_characterManager across level resets is unreliable:
        // TH20.Level may be a plain C# class (not UnityEngine.Object), so Unity's
        // destroyed-object null check doesn't apply and stale references go undetected.
        // Initialise() is ~10 reflection calls — sub-millisecond — so calling it every
        // time is safe and guarantees fresh references after any reset or reload.
        private static void EnsureValid()
        {
            Initialise();
        }

        public static void Initialise()
        {
            try
            {
                _app = null;
                _level = null;
                _financeManager = null;
                _characterManager = null;
                _initialised = false;

                var onlineManagerType = Type.GetType("TH20.OnlineManager, Assembly-CSharp");
                var appField = onlineManagerType?.GetField("_app", BindingFlags.Static | BindingFlags.NonPublic);
                _app = appField?.GetValue(null);

                if (_app != null)
                    _level = _app.GetType().GetProperty("Level", BindingFlags.Instance | BindingFlags.Public)?.GetValue(_app);

                if (_level != null)
                {
                    var lt = _level.GetType();
                    _financeManager   = lt.GetProperty("FinanceManager",   BindingFlags.Instance | BindingFlags.Public)?.GetValue(_level);
                    _characterManager = lt.GetProperty("CharacterManager", BindingFlags.Instance | BindingFlags.Public)?.GetValue(_level);
                    _initialised = _financeManager != null && _characterManager != null;
                }

                Debug.Log(_initialised
                    ? "[TikTokMod] GameInterface Initialised Successfully!"
                    : "[TikTokMod] Failed to initialise GameInterface.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TikTokMod] GameInterface Init Error: {ex.Message}");
            }
        }

        public static void AddMoney(int amount)
        {
            EnsureValid();
            if (_financeManager == null) return;

            try
            {
                var fmType = _financeManager.GetType();

                // Try AddBalance overloads, picking the 2-param (int, int) version
                var addBalance = fmType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "AddBalance" && m.GetParameters().Length == 2);

                if (addBalance != null)
                {
                    var p = addBalance.GetParameters();
                    // First param might be an enum cast to int — try 0 first (main cash)
                    addBalance.Invoke(_financeManager, new object[] { Convert.ChangeType(0, p[0].ParameterType), amount });
                    Debug.Log($"[TikTokMod] AddBalance(0, {amount}) called");
                    return;
                }

                // Fallback: directly modify Balance property
                var balanceProp = fmType.GetProperty("Balance", BindingFlags.Instance | BindingFlags.Public);
                if (balanceProp != null)
                {
                    int current = (int)balanceProp.GetValue(_financeManager);
                    balanceProp.SetValue(_financeManager, current + amount);
                    Debug.Log($"[TikTokMod] Balance property set to {current + amount}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TikTokMod] AddMoney Error: {ex}");
            }
        }

        public static void SpawnStaffMember(string role, string displayName, string avatarPath, TikTokPlugin plugin)
        {
            EnsureValid();
            if (_characterManager == null)
            {
                Debug.LogWarning("[TikTokMod] CharacterManager not ready for staff spawn.");
                return;
            }

            try
            {
                var cmType = _characterManager.GetType();

                // ── 1. Get JobApplicantManager ────────────────────────────────
                var levelType = _level.GetType();
                var jobAppMgr = levelType
                    .GetProperty("JobApplicantManager", BindingFlags.Instance | BindingFlags.Public)
                    ?.GetValue(_level);
                if (jobAppMgr == null) { Debug.LogError("[TikTokMod] JobApplicantManager not found"); return; }

                var jamType = jobAppMgr.GetType();

                // ── 2. Find the matching pool by key.ToString() ───────────────
                var poolsField = jamType.GetField("_jobApplicantPools", BindingFlags.Instance | BindingFlags.NonPublic);
                var pools = poolsField?.GetValue(jobAppMgr) as System.Collections.IDictionary;

                object pool = null;
                if (pools != null)
                    foreach (System.Collections.DictionaryEntry entry in pools)
                        if ((entry.Key?.ToString() ?? "").Equals(role, StringComparison.OrdinalIgnoreCase))
                        { pool = entry.Value; break; }

                if (pool == null)
                { Debug.LogError($"[TikTokMod] No pool found for role '{role}'"); return; }

                var poolType = pool.GetType();

                // ── 3. Generate a fresh applicant via AddApplicant ────────────
                var applicantsProp  = poolType.GetProperty("Applicants", BindingFlags.Instance | BindingFlags.Public);
                var addApplicant    = poolType.GetMethod("AddApplicant",  BindingFlags.Instance | BindingFlags.Public);
                var removeApplicant = poolType.GetMethod("RemoveApplicant", BindingFlags.Instance | BindingFlags.Public);

                // Deps from JobApplicantManager private fields
                var qualifications  = jamType.GetField("_qualifications",          BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(jobAppMgr);
                var traitsManager   = jamType.GetField("_characterTraitsManager",  BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(jobAppMgr);
                var metagame        = levelType.GetProperty("Metagame", BindingFlags.Instance | BindingFlags.Public)?.GetValue(_level);

                int countBefore = (applicantsProp?.GetValue(pool) as System.Collections.IList)?.Count ?? 0;

                Debug.Log($"[TikTokMod] AddApplicant deps — qualifications: {qualifications?.GetType().Name ?? "NULL"}, " +
                          $"traitsManager: {traitsManager?.GetType().Name ?? "NULL"}, " +
                          $"metagame: {metagame?.GetType().Name ?? "NULL"}");

                // Try to get recruitmentFeePercentage array from Config
                // Fall back to a zero array with 5 slots (one per rank tier)
                var configField = jamType.GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic);
                var config = configField?.GetValue(jobAppMgr);
                float[] feePercentages = null;
                if (config != null)
                {
                    var feeProp = config.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .FirstOrDefault(p => p.Name.IndexOf("recruitment", StringComparison.OrdinalIgnoreCase) >= 0
                                          || p.Name.IndexOf("fee",         StringComparison.OrdinalIgnoreCase) >= 0
                                          || p.Name.IndexOf("percentage",  StringComparison.OrdinalIgnoreCase) >= 0);
                    if (feeProp != null)
                        feePercentages = feeProp.GetValue(config) as float[];
                    Debug.Log($"[TikTokMod] Config fee prop: {feeProp?.Name ?? "not found"}, value: {(feePercentages == null ? "null" : string.Join(",", feePercentages))}");
                }
                if (feePercentages == null) feePercentages = new float[] { 0f, 0f, 0f, 0f, 0f };

                // AddApplicant(Single[] recruitmentFeePercentage, WeightedList qualifications, CharacterTraitsManager, Metagame, Level)
                addApplicant?.Invoke(pool, new object[] { feePercentages, qualifications, traitsManager, metagame, _level });

                var applicants = applicantsProp?.GetValue(pool) as System.Collections.IList;
                if (applicants == null || applicants.Count == 0)
                { Debug.LogError($"[TikTokMod] AddApplicant produced no applicant for '{role}'"); return; }

                // Take the newest applicant (last in list)
                object applicant = applicants[applicants.Count - 1];
                Debug.Log($"[TikTokMod] Generated applicant: {applicant}");

                // ── 4. SpawnStaff(JobApplicant, Vector3, bool) ────────────────
                var spawnMethod = cmType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "SpawnStaff"
                        && m.GetParameters().Length == 3
                        && m.GetParameters()[0].ParameterType.Name == "JobApplicant");

                if (spawnMethod == null) { Debug.LogError("[TikTokMod] SpawnStaff not found"); return; }

                // Snapshot staff before spawn so we can find the new one
                var staffBefore = GetAllStaff();

                Vector3 spawnPos = FindValidSpawnPosition(staffBefore);
                spawnMethod.Invoke(_characterManager, new object[] { applicant, spawnPos, false });
                Debug.Log($"[TikTokMod] SpawnStaff called: {role} '{displayName}'");

                // Capture the remove action but defer it: calling it immediately after SpawnStaff
                // can corrupt the staff member's hire record if the game still references the
                // applicant during async initialisation, causing the staff to be de-listed
                // while their GameObject remains (visible-but-stuck ghost).
                Action removeAction = removeApplicant != null
                    ? () => removeApplicant.Invoke(pool, new object[] { applicant })
                    : (Action)null;

                // Wait for the new staff member to appear, then name + decorate
                plugin.StartCoroutine(WaitAndDecorateStaff(staffBefore, displayName, avatarPath, plugin, removeAction));
            }
            catch (Exception ex)
            {
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                Debug.LogError($"[TikTokMod] SpawnStaffMember Error: {inner.GetType().Name}: {inner.Message}\n{inner.StackTrace}");
            }
        }

        // Find a clear world position for spawning a staff member.
        // Samples near an existing character position so it's guaranteed to be
        // inside the building on a walkable NavMesh tile.
        private static Vector3 FindValidSpawnPosition(HashSet<object> existingStaff)
        {
            // 1. Try existing staff members
            foreach (var s in existingStaff)
            {
                Vector3? pos = TryGetCharacterPosition(s);
                if (pos.HasValue)
                {
                    Vector3 candidate = RandomNearby(pos.Value);
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(candidate, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                        return hit.position;
                    return pos.Value; // offset not on navmesh — use the exact staff position
                }
            }

            // 2. Try existing patients
            foreach (var p in GetAllPatients(out _))
            {
                Vector3? pos = TryGetCharacterPosition(p);
                if (pos.HasValue) return pos.Value;
            }

            // 3. NavMesh sample near world origin (wide radius)
            {
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(Vector3.zero, out hit, 100f, UnityEngine.AI.NavMesh.AllAreas))
                    return hit.position;
            }

            // 4. Absolute fallback
            Debug.LogWarning("[TikTokMod] FindValidSpawnPosition: could not find a valid point; using Vector3.zero");
            return Vector3.zero;
        }

        // If a character ended up off the NavMesh (e.g. on a road or inside furniture),
        // teleport them to the nearest walkable point.  Used as a post-spawn correction
        // for patients who sometimes materialise at the wrong world position.
        private static void CorrectPositionIfOffNavMesh(object character)
        {
            try
            {
                Transform t = TryGetCharacterTransform(character);
                if (t == null) return;

                UnityEngine.AI.NavMeshHit hit;
                // If they're already on the NavMesh within 0.3 m, leave them alone
                if (UnityEngine.AI.NavMesh.SamplePosition(t.position, out hit, 0.3f, UnityEngine.AI.NavMesh.AllAreas))
                    return;

                // They're off — find the nearest walkable point within a generous radius
                if (UnityEngine.AI.NavMesh.SamplePosition(t.position, out hit, 50f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    Debug.Log($"[TikTokMod] Patient off-navmesh at {t.position}; correcting to {hit.position}");
                    t.position = hit.position;
                    return;
                }

                // Wider fallback: use an existing character's position
                var fallback = FindValidSpawnPosition(GetAllStaff());
                Debug.Log($"[TikTokMod] Patient off-navmesh — using fallback position {fallback}");
                t.position = fallback;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TikTokMod] CorrectPositionIfOffNavMesh: {ex.Message}");
            }
        }

        private static Vector3? TryGetCharacterPosition(object character)
        {
            Transform t = TryGetCharacterTransform(character);
            return t != null ? (Vector3?)t.position : null;
        }

        private static Transform TryGetCharacterTransform(object character)
        {
            try
            {
                var ct = character.GetType();
                return ct.GetProperty("Transform", BindingFlags.Instance | BindingFlags.Public)?.GetValue(character) as Transform
                    ?? (ct.GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public)?.GetValue(character) as GameObject)?.transform
                    ?? (character as Component)?.transform;
            }
            catch { return null; }
        }

        // Return a position within ~1 m of the source, at the same height.
        private static Vector3 RandomNearby(Vector3 source)
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float dist  = UnityEngine.Random.Range(0.5f, 1.5f);
            return source + new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
        }

        private static HashSet<object> GetAllStaff()
        {
            var set = new HashSet<object>();
            if (_characterManager == null) return set;
            var list = _characterManager.GetType()
                .GetProperty("StaffMembers", BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(_characterManager) as System.Collections.IEnumerable;
            if (list != null) foreach (var s in list) set.Add(s);
            return set;
        }

        private static IEnumerator WaitAndDecorateStaff(
            HashSet<object> staffBefore, string displayName, string avatarPath, TikTokPlugin plugin,
            Action onFound = null)
        {
            float elapsed = 0f;
            object newStaff = null;

            while (elapsed < 30f)
            {
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;

                foreach (var s in GetAllStaff())
                {
                    if (!staffBefore.Contains(s)) { newStaff = s; break; }
                }
                if (newStaff != null) break;
            }

            if (newStaff == null)
            {
                Debug.LogWarning($"[TikTokMod] New staff member not found within 30 s.");
                yield break;
            }

            // Staff confirmed in list — safe to remove from applicant pool now.
            try { onFound?.Invoke(); }
            catch (Exception ex) { Debug.LogWarning($"[TikTokMod] removeApplicant deferred call failed: {ex.Message}"); }

            // Register for orphan tracking
            _trackedChars[newStaff] = displayName;

            yield return new WaitForSeconds(1f);

            // Rename
            try
            {
                newStaff.GetType()
                    .GetMethod("SetUserSpecifiedName", BindingFlags.Instance | BindingFlags.Public)
                    ?.Invoke(newStaff, new object[] { displayName });
                Debug.Log($"[TikTokMod] Staff renamed to {displayName}");
            }
            catch (Exception ex) { Debug.LogWarning($"[TikTokMod] Staff rename failed: {ex.Message}"); }

            // Persist avatar and attach billboard
            if (!string.IsNullOrEmpty(avatarPath))
            {
                PersistAvatar(newStaff, avatarPath, displayName, "staff");
                plugin.LoadTextureFromFile(avatarPath, tex =>
                    AttachAvatarBillboard(newStaff, tex, displayName));
            }
        }

        public static void SpawnFollowerPatient(string displayName, string avatarUrl, TikTokPlugin plugin)
        {
            EnsureValid();
            if (_characterManager == null)
            {
                Debug.LogWarning("[TikTokMod] CharacterManager not ready — is a hospital loaded?");
                return;
            }

            try
            {
                var cmType = _characterManager.GetType();

                object illness = cmType
                    .GetMethod("RandomIllness", BindingFlags.Instance | BindingFlags.Public)
                    ?.Invoke(_characterManager, null);

                object arrivalMethod = cmType
                    .GetMethod("GetDefaultArrivalMethod", BindingFlags.Instance | BindingFlags.Public)
                    ?.Invoke(_characterManager, null);

                if (illness == null || arrivalMethod == null)
                {
                    Debug.LogError("[TikTokMod] Could not get illness or arrival method.");
                    return;
                }

                // Snapshot current patients so we can find the new one after spawn
                var patientsBefore = GetAllPatients(out string usedProp);
                Debug.Log($"[TikTokMod] Pre-spawn snapshot: {patientsBefore.Count} patients via '{usedProp}'");

                // SpawnPatient(IllnessDefinition, ArrivalMethodDefinition, IPatientSpawned, bool)
                var spawnMethod = cmType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "SpawnPatient"
                        && m.GetParameters().Length >= 2
                        && m.GetParameters()[0].ParameterType.Name == "IllnessDefinition");

                if (spawnMethod == null)
                {
                    Debug.LogError("[TikTokMod] SpawnPatient method not found.");
                    return;
                }

                var args = new object[spawnMethod.GetParameters().Length];
                args[0] = illness;
                args[1] = arrivalMethod;
                spawnMethod.Invoke(_characterManager, args);

                Debug.Log($"[TikTokMod] SpawnPatient called for {displayName}");

                // Poll for the new patient and name + decorate it
                plugin.StartCoroutine(WaitAndDecoratePatient(patientsBefore, usedProp, displayName, avatarUrl, plugin));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TikTokMod] SpawnFollowerPatient Error: {ex.Message}");
            }
        }

        // Try several CharacterManager properties to get a patient list.
        // Returns the set and the property name that worked (for logging).
        private static HashSet<object> GetAllPatients(out string usedProp)
        {
            usedProp = "none";
            if (_characterManager == null) return new HashSet<object>();

            string[] candidates = { "Patients", "SpawnedPatients", "AllCharacters" };
            var cmType = _characterManager.GetType();

            foreach (var name in candidates)
            {
                try
                {
                    var prop = cmType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                    if (prop == null) continue;
                    var list = prop.GetValue(_characterManager) as System.Collections.IEnumerable;
                    if (list == null) continue;
                    var set = new HashSet<object>();
                    foreach (var p in list) set.Add(p);
                    if (set.Count > 0 || name == candidates[candidates.Length - 1])
                    {
                        usedProp = name;
                        return set;
                    }
                }
                catch { }
            }
            return new HashSet<object>();
        }

        private static IEnumerator WaitAndDecoratePatient(
            HashSet<object> patientsBefore, string propName, string displayName, string avatarUrl, TikTokPlugin plugin)
        {
            // Poll up to 45 s for a new patient to appear
            float elapsed = 0f;
            object newPatient = null;

            while (elapsed < 45f)
            {
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;

                var current = GetAllPatients(out _);
                foreach (var p in current)
                {
                    if (!patientsBefore.Contains(p))
                    {
                        newPatient = p;
                        break;
                    }
                }
                if (newPatient != null) break;

                // Log progress every 5 s so we can see what's in the list
                if (elapsed % 5f < 0.6f)
                    Debug.Log($"[TikTokMod] Waiting for patient... {elapsed:F0}s, current count: {GetAllPatients(out _).Count}");
            }

            if (newPatient == null)
            {
                Debug.LogWarning($"[TikTokMod] New patient not found within 45 s (used prop: {propName}).");
                yield break;
            }

            // Register for orphan tracking
            _trackedChars[newPatient] = displayName;

            // Give the patient a moment to fully initialise its components
            yield return new WaitForSeconds(1f);

            // If the patient ended up off the NavMesh (e.g. stuck on the road),
            // warp them to the nearest walkable position before anything else.
            CorrectPositionIfOffNavMesh(newPatient);

            // Rename the patient
            try
            {
                newPatient.GetType()
                    .GetMethod("SetUserSpecifiedName", BindingFlags.Instance | BindingFlags.Public)
                    ?.Invoke(newPatient, new object[] { displayName });
                Debug.Log($"[TikTokMod] Patient renamed to {displayName}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TikTokMod] Rename failed: {ex.Message}");
            }

            // Persist avatar and attach billboard
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                string persistPath = PersistAvatar(newPatient, avatarUrl, displayName, "patient");
                plugin.LoadTextureFromFile(avatarUrl, tex =>
                    AttachAvatarBillboard(newPatient, tex, displayName));
            }
        }

        // ── Ghost / Kill / Fire ───────────────────────────────────────

        public static void SpawnGhost()
        {
            EnsureValid();
            if (_characterManager == null) { Debug.LogWarning("[TikTokMod] CharacterManager not ready for ghost spawn."); return; }
            try
            {
                var cmType = _characterManager.GetType();

                // Try the most likely method names for spawning a ghost
                foreach (var name in new[] { "SpawnRandomGhost", "SpawnGhostFromCharacter", "SpawnGhost", "CreateGhost", "AddGhost", "SpawnNewGhost" })
                {
                    var m = cmType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                  .FirstOrDefault(x => x.Name == name);
                    if (m == null) continue;

                    var parms  = m.GetParameters();
                    var args   = new object[parms.Length]; // nulls / defaults are fine for optional params
                    // First param is often a position
                    if (parms.Length > 0 && parms[0].ParameterType == typeof(Vector3))
                        args[0] = FindValidSpawnPosition(GetAllStaff());

                    m.Invoke(_characterManager, args);
                    Debug.Log($"[TikTokMod] Ghost spawned via {name}()");
                    return;
                }

                Debug.LogWarning("[TikTokMod] SpawnGhost: no suitable method found on CharacterManager. " +
                    "Available public methods: " +
                    string.Join(", ", cmType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                        .Where(x => x.Name.ToLower().Contains("ghost") || x.Name.ToLower().Contains("spirit"))
                        .Select(x => x.Name)));
            }
            catch (Exception ex) { Debug.LogError($"[TikTokMod] SpawnGhost Error: {ex.Message}"); }
        }

        public static void KillRandomPatient()
        {
            EnsureValid();
            if (_characterManager == null) { Debug.LogWarning("[TikTokMod] CharacterManager not ready."); return; }
            try
            {
                var patients = GetAllPatients(out _).ToList();
                if (patients.Count == 0) { Debug.LogWarning("[TikTokMod] KillRandomPatient: no patients found."); return; }

                var target = patients[UnityEngine.Random.Range(0, patients.Count)];
                var pt     = target.GetType();

                // Try common kill method names
                foreach (var name in new[] { "Kill", "Die", "ForceDie", "SetDead", "Death", "ForceKill" })
                {
                    var m = pt.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                               .FirstOrDefault(x => x.Name == name && x.GetParameters().Length == 0);
                    if (m == null) continue;
                    m.Invoke(target, null);
                    Debug.Log($"[TikTokMod] Killed patient via {pt.Name}.{name}()");
                    return;
                }

                // Fallback: set health to 0 via property
                var healthProp = pt.GetProperty("Health", BindingFlags.Instance | BindingFlags.Public)
                              ?? pt.GetProperty("CurrentHealth", BindingFlags.Instance | BindingFlags.Public);
                if (healthProp != null && healthProp.CanWrite)
                {
                    healthProp.SetValue(target, Convert.ChangeType(0, healthProp.PropertyType));
                    Debug.Log($"[TikTokMod] Set patient health to 0 via {healthProp.Name}");
                    return;
                }

                Debug.LogWarning("[TikTokMod] KillRandomPatient: no kill method found. " +
                    "Available methods containing 'kill'/'die'/'death': " +
                    string.Join(", ", pt.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(x => x.Name.ToLower().Contains("kill") || x.Name.ToLower().Contains("die") || x.Name.ToLower().Contains("death"))
                        .Select(x => x.Name)));
            }
            catch (Exception ex) { Debug.LogError($"[TikTokMod] KillRandomPatient Error: {ex.Message}"); }
        }

        public static void FireRandomStaff()
        {
            EnsureValid();
            if (_characterManager == null) { Debug.LogWarning("[TikTokMod] CharacterManager not ready."); return; }
            try
            {
                var staff = GetAllStaff().ToList();
                if (staff.Count == 0) { Debug.LogWarning("[TikTokMod] FireRandomStaff: no staff found."); return; }

                var target = staff[UnityEngine.Random.Range(0, staff.Count)];
                var st     = target.GetType();

                // Try common fire/dismiss method names
                foreach (var name in new[] { "Fire", "Dismiss", "Sack", "Remove", "Fired", "ForceFire", "ForceDismiss" })
                {
                    var m = st.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                               .FirstOrDefault(x => x.Name == name && x.GetParameters().Length == 0);
                    if (m == null) continue;
                    m.Invoke(target, null);
                    Debug.Log($"[TikTokMod] Staff fired via {st.Name}.{name}()");
                    return;
                }

                // Fallback: try CharacterManager.FireStaff(staff)
                var cmType = _characterManager.GetType();
                foreach (var name in new[] { "FireStaff", "DismissStaff", "RemoveStaff", "SackStaff" })
                {
                    var m = cmType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                  .FirstOrDefault(x => x.Name == name && x.GetParameters().Length == 1);
                    if (m == null) continue;
                    m.Invoke(_characterManager, new[] { target });
                    Debug.Log($"[TikTokMod] Staff fired via CharacterManager.{name}()");
                    return;
                }

                Debug.LogWarning("[TikTokMod] FireRandomStaff: no fire method found. " +
                    "Available methods containing 'fire'/'dismiss'/'sack': " +
                    string.Join(", ", st.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(x => x.Name.ToLower().Contains("fire") || x.Name.ToLower().Contains("dismiss") || x.Name.ToLower().Contains("sack"))
                        .Select(x => x.Name)));
            }
            catch (Exception ex) { Debug.LogError($"[TikTokMod] FireRandomStaff Error: {ex.Message}"); }
        }

        private static List<object> ToList(HashSet<object> set) => new List<object>(set);

        private static void AttachAvatarBillboard(object patient, Texture2D tex, string displayName)
        {
            try
            {
                // Get the patient's Unity Transform.
                // TPH's patient classes wrap a GameObject rather than extending MonoBehaviour,
                // so we try several property names before giving up.
                Transform transform = null;
                var pt = patient.GetType();

                // 1. "Transform" (capital T) — most likely in TPH wrapper classes
                transform = pt.GetProperty("Transform", BindingFlags.Instance | BindingFlags.Public)
                              ?.GetValue(patient) as Transform;

                // 2. Via "GameObject" property → .transform
                if (transform == null)
                {
                    var go = pt.GetProperty("GameObject", BindingFlags.Instance | BindingFlags.Public)
                               ?.GetValue(patient) as GameObject;
                    transform = go?.transform;
                }

                // 3. Lowercase "transform" (standard MonoBehaviour path)
                if (transform == null)
                    transform = pt.GetProperty("transform", BindingFlags.Instance | BindingFlags.Public)
                                  ?.GetValue(patient) as Transform;

                // 4. Direct cast (if patient is itself a Component)
                if (transform == null)
                    transform = (patient as Component)?.transform;

                if (transform == null)
                {
                    Debug.LogWarning("[TikTokMod] Could not get patient transform for billboard.");
                    return;
                }

                // Create billboard parent
                var billboard = new GameObject("TikTokAvatar_" + displayName);
                billboard.transform.SetParent(transform, false);
                billboard.transform.localPosition = new Vector3(0f, 2.4f, 0f);
                billboard.transform.localScale    = new Vector3(1.0f, 1.0f, 1.0f);

                // Sprite background quad
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(billboard.transform, false);
                quad.transform.localPosition = Vector3.zero;

                // Apply avatar texture via a simple material
                var renderer = quad.GetComponent<MeshRenderer>();
                var mat = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture"));
                mat.mainTexture = tex;
                renderer.material = mat;

                // Make it face the camera every frame
                billboard.AddComponent<BillboardFaceCamera>();

                Debug.Log($"[TikTokMod] Avatar billboard attached for {displayName}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TikTokMod] Billboard error: {ex.Message}");
            }
        }
    }

    // Rotates the object to always face the main camera
    public class BillboardFaceCamera : MonoBehaviour
    {
        void LateUpdate()
        {
            if (Camera.main != null)
                transform.forward = Camera.main.transform.forward;
        }
    }
}
