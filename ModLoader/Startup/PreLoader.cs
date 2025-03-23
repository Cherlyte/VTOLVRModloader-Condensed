using System;
using System.Reflection;
using Serilog;

namespace ModLoader.Startup
{
    internal static class PreLoader
    {
        public static void SetupLogging()
        {
            var currentTime = DateTime.Now;
            var logPath = $"@Mod Loader/Logs/GameLog_{currentTime.Ticks}.txt";
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logPath)
                .CreateLogger();
            
            Log.Information("Started at {CurrentTime}", currentTime);
            
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        }

        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            string path = string.Empty;
            try
            {
                path = args.LoadedAssembly.Location;
            }
            catch (Exception)
            {
                path = "(In Memory Assembly)";
            }
            Log.Information("{AssemblyFullName} loaded from {AssemblyLocation}",args.LoadedAssembly.FullName, path);
        }

        public static void LoadHarmony()
        {
            Log.Information("Loading Harmony");
            try
            {
                Assembly.LoadFrom(@"@Mod Loader\Managed\0Harmony.dll");
            }
            catch (Exception e)
            {
                Log.Error(e,"Failed to load Harmony");
                return;
            }
            PostLoader.PatchHarmony();
        }
    }
}