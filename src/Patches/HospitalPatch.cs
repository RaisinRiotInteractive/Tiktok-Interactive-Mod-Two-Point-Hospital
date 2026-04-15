using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;

namespace TPH_TikTokMod.Patches
{
    // Fires when loading from a save file.
    [HarmonyPatch(typeof(TH20.Level), "RestoreFromSave")]
    public class LevelPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Debug.Log("[TikTokMod] Level Loaded (RestoreFromSave)! Initialising GameInterface...");
            GameInterface.Initialise();
            GameInterface.ReapplyAllAvatars(TikTokPlugin.Instance);
        }
    }

    // Applies at runtime any additional Level lifecycle methods that should also
    // trigger a re-initialise (e.g. fresh level start, restart).  Called from
    // TikTokPlugin.Awake() after harmony.PatchAll() so we can handle missing
    // method names without crashing the whole patch pass.
    public static class LevelLifecyclePatcher
    {
        private static readonly string[] _candidates = { "Initialise", "NewGame", "StartLevel", "Setup" };

        public static void TryPatchFreshStart(Harmony harmony)
        {
            var levelType = typeof(TH20.Level);
            var postfix   = new HarmonyMethod(typeof(LevelLifecyclePatcher), nameof(FreshStartPostfix));

            foreach (var name in _candidates)
            {
                try
                {
                    var method = levelType.GetMethod(name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null) continue;

                    harmony.Patch(method, postfix: postfix);
                    Debug.Log($"[TikTokMod] Patched TH20.Level.{name} for fresh-start detection.");
                    return; // patch the first one we find
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[TikTokMod] Could not patch TH20.Level.{name}: {ex.Message}");
                }
            }

            Debug.LogWarning("[TikTokMod] No fresh-start Level method found; relying on stale-reference check only.");
        }

        public static void FreshStartPostfix()
        {
            Debug.Log("[TikTokMod] Level fresh-start detected — initialising GameInterface.");
            GameInterface.Initialise();
        }
    }
}
