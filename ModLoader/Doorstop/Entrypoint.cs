// This HAS to be Doorstop.Entrypoint.Start for Doorstop to find the entry point.
// See https://github.com/NeighTools/UnityDoorstop#minimal-injection-example

using System.IO;
using System.Reflection;
using ModLoader.Startup;

namespace Doorstop
{
    class Entrypoint
    {
        private static string[] _coreDependencies = { "Serilog.dll", "Serilog.Sinks.File.dll" };
        private static string _coreDependenciesPath = @"@Mod Loader\Managed";
        public static void Start()
        {
            LoadCoreDependencies();
            PreLoader.SetupLogging();
            PreLoader.LoadHarmony();
        }

        private static void LoadCoreDependencies()
        {
            foreach (var filePath in _coreDependencies)
            {
                Assembly.LoadFrom(Path.Combine(_coreDependenciesPath, filePath));
            }
        }
    }
}