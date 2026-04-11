using HarmonyLib;
using UnityEngine;
using System;

namespace TPH_TikTokMod.Patches
{
    [HarmonyPatch(typeof(TH20.Level), "RestoreFromSave")]
    public class LevelPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Debug.Log("[TikTokMod] Level Loaded! Initialising GameInterface...");
            GameInterface.Initialise();
            GameInterface.ReapplyAllAvatars(TikTokPlugin.Instance);
        }
    }
}
