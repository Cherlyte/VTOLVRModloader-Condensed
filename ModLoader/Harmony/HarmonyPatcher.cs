using UnityEngine;

namespace ModLoader.Harmony
{
    internal static class HarmonyPatcher
    {
        public static void PatchGame()
        {
            Debug.Log("Applying the Mod Loader's Harmony Patches");
            var harmony = new HarmonyLib.Harmony("vtolvr.modloader");
            harmony.PatchAll();
            Debug.Log("Applied");
        }
    }
}