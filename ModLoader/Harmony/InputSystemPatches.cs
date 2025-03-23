using System;
using HarmonyLib;
using ModLoader.Startup;
using UnityEngine;

namespace ModLoader.Harmony
{
    // For some reason the game was crashing when calling this method, so I just block it and return a fake value
    [HarmonyPatch(typeof(UnityEngine.InputSystem.InputSystem), "get_version")]
    [HarmonyPatchCategory(PostLoader.PostStartUpCategory)]
    public class InputSystemPatches
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Version __result)
        {
            Debug.Log($"{nameof(InputSystemPatches)} has prefixed get version");
            __result = new Version(99, 99, 99);
            return false;
        }
    }
}