using System;
using System.IO;
using System.IO.Abstractions;
using Mod_Manager.Abstractions;
using Mod_Manager.Abstractions.ConfigParser;
using Serilog;

namespace Mod_Manager.Models;

internal class DoorstopManager : IDoorstopManager
{
    private const string _fileName = "doorstop_config.ini";
    private const string _defaultContents = @"# General options for Unity Doorstop
[General]

# Enable Doorstop?
enabled=false

# Path to the assembly to load and execute
# NOTE: The entrypoint must be of format `static void Doorstop.Entrypoint.Start()`
target_assembly=@Mod Loader\Managed\Mod Loader.dll

# If true, Unity's output log is redirected to <current folder>\output_log.txt
redirect_output_log=false

# If enabled, DOORSTOP_DISABLE env var value is ignored
# USE THIS ONLY WHEN ASKED TO OR YOU KNOW WHAT THIS MEANS
ignore_disable_switch=false


# Options specific to running under Unity Mono runtime
[UnityMono]

# Overrides default Mono DLL search path
# Sometimes it is needed to instruct Mono to seek its assemblies from a different path
# (e.g. mscorlib is stripped in original game)
# This option causes Mono to seek mscorlib and core libraries from a different folder before Managed
# Original Managed folder is added as a secondary folder in the search path
dll_search_path_override=@Mod Loader\Managed

# If true, Mono debugger server will be enabled
debug_enabled=false

# When debug_enabled is true, specifies the address to use for the debugger server
debug_address=127.0.0.1:10000

# If true and debug_enabled is true, Mono debugger server will suspend the game execution until a debugger is attached
debug_suspend=false

# Options sepcific to running under Il2Cpp runtime
[Il2Cpp]

# Path to coreclr.dll that contains the CoreCLR runtime
coreclr_path=

# Path to the directory containing the managed core libraries for CoreCLR (mscorlib, System, etc.)
corlib_dir=";
    private readonly IFileSystem _fileSystem;
    private readonly IFileManager _fileManager;
    private readonly IConfigParser _configParser;

    public DoorstopManager(IFileSystem fileSystem, IFileManager fileManager, IConfigParser configParser)
    {
        _fileSystem = fileSystem;
        _fileManager = fileManager;
        _configParser = configParser;
    }

    public void CheckForOldFile()
    {
        var vtolPath = _fileManager.GetVtolDirectory();
        var filePath = Path.Combine(vtolPath, _fileName);
        if (!_fileSystem.File.Exists(filePath))
        {
            CreateDefaultFile();
            return;
        }

        _configParser.SetFile(filePath);
        if (_configParser.SectionExists("UnityDoorstop"))
        {
            _fileSystem.File.Delete(filePath);
            CreateDefaultFile();
            return;
        }

        if (!_configParser.SectionExists("General"))
        {
            Log.Warning("The config file did not have UnityDoorstop or General section defined");
            return;
        }

        var targetAssembly = _configParser.GetValue("General", "target_assembly", string.Empty);
        if (!string.IsNullOrEmpty(targetAssembly) && targetAssembly.Contains("VTPatcher.dll", StringComparison.OrdinalIgnoreCase))
        {
            _fileSystem.File.Delete(filePath);
            CreateDefaultFile();
            return;
        }
            
        var isEnabled = _configParser.GetValue("General", "enabled", true);
        if (!isEnabled)
        {
            return;
        }

        // We use the launch options to enable the mod loader
        //_configParser.SetValue("General", "enabled", false); --Disabled for obvious reasons.
        _configParser.Save();
    }

    private void CreateDefaultFile()
    {
        var vtol = _fileManager.GetVtolDirectory();
        var filePath = Path.Combine(vtol, _fileName);
        _fileSystem.File.WriteAllText(filePath, _defaultContents);
        _configParser.SetFile(filePath);
    }
}