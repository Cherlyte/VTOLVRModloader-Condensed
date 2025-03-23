using System.Reflection;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using ModLoader.Startup;
using Serilog;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace ModLoader.Harmony
{
    [HarmonyPatch(typeof(SplashSceneController), nameof(SplashSceneController.Start))]
    [HarmonyPatchCategory(PostLoader.StartUpCategory)]
    public class SplashSceneController_Start
    {
        public static void Prefix()
        {
            CrashReportHandler.enableCaptureExceptions = false;
            if (GameStartup.version.releaseType != GameVersion.ReleaseTypes.Modded)
            {
                GameStartup._version.releaseType = GameVersion.ReleaseTypes.Modded;
            }
            Log.Information("GameVersion: {GameVersion}", GameStartup.version);
            GameStartup._versionSet = false;
            Debug.Log("Changed to version:" + GameStartup.versionString);

            Debug.Log("Starting UniTask");
            var initMethod = typeof(PlayerLoopHelper).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic);
            initMethod.Invoke(null, null);
            
            Debug.Log("Patching post start up methods");
            PostLoader.Harmony.PatchCategory(PostLoader.PostStartUpCategory);
            new GameObject(nameof(TypeInstantiator), typeof(TypeInstantiator));
        }
    }
}