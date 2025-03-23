using System;
using Serilog;

namespace ModLoader.Startup
{
    internal static class PostLoader
    {
        public const string StartUpCategory = "StartUp";
        public const string PostStartUpCategory = "PostStartUp";
        public static HarmonyLib.Harmony Harmony;
        
        public static void PatchHarmony()
        {
            Log.Information("Patching Harmony");
            Harmony = new HarmonyLib.Harmony("vtolvr.modloader");
            try
            {
                Harmony.PatchCategory(StartUpCategory);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to Patch Category '{Category}'", StartUpCategory);
            }
        }
    }
}