using System.Collections;
using HarmonyLib;
using ModLoader.Assets;
using ModLoader.Startup;
using UnityEngine;

namespace ModLoader.Harmony
{
    [HarmonyPatch(typeof(VTResources.AsyncVehicleLoader), nameof(VTResources.AsyncVehicleLoader.LoadRoutine))]
    [HarmonyPatchCategory(PostLoader.StartUpCategory)]
    public class AsyncVehicleLoaderPatches
    {
        [HarmonyPostfix]
        public static void Postfix(ref AsyncOpStatus p, ref IEnumerator __result)
        {
            var enumerator = new PrefixEnumerator<AsyncOpStatus>(__result, LoadModLoaderAssets, p);
            __result = enumerator.GetEnumerator();
        }

        private static IEnumerator LoadModLoaderAssets(AsyncOpStatus status)
        {
            var assets = GameObject.FindObjectOfType<AssetBundleLoader>();
            Debug.Log("Loading Mod Loader's assets");
            status.status = "Loading Mod Loader...";
            Debug.Log($"Heya, status is {status.status}");
            Debug.Log($"assets: {assets}, status: {status}");
            yield return assets.LoadAssets(status);
            Debug.Log("Finished loading Mod Loader's assets");
            
            assets.SpawnPrefab("Modded Warning Text", 0, 0, 10);
            assets.SpawnPrefab("Modded Warning Text", posZ:-10, rotY: 180);
            assets.SpawnPrefab("Modded Warning Text", posX: 10, rotY: 90);
            assets.SpawnPrefab("Modded Warning Text", posX: -10, rotY: -90);
        }
    }
}