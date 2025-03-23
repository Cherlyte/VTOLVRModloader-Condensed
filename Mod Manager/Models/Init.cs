using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Mod_Manager.Abstractions;
using Serilog;

namespace Mod_Manager.Models;

internal sealed class Init : IInit
{
    private const string _doorstopConfig = "doorstop_config.ini";
    private readonly IFileSystem _fileSystem;
    private FilesState _currentState = FilesState.Unknown;

    public Init(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Searches the parent folder for a valid VTOL VR directory.
    /// </summary>
    /// <returns>Returns true if it is installed inside VTOL VR's game files</returns>
    public bool IsInGameFiles()
    {
        var modLoaderDirectory =
            _fileSystem.DirectoryInfo.New(_fileSystem.Directory.GetCurrentDirectory());
        var vtolDirectory = modLoaderDirectory.Parent.Parent;

        return IsVtolFolder(vtolDirectory);
    }
    
    public bool IsVtolFolder(IDirectoryInfo folder)
    {
        // Checks for 'VTOLVR.exe' and 'VTOLVR_Data' in the root
        
        const string vtolExeName = "VTOLVR.exe";
        if (!folder.GetFiles().Any(f => f.Name.Equals(vtolExeName)))
        {
            return false;
        }

        const string dataFolderName = "VTOLVR_Data";
        var subFolders = folder.GetDirectories();
        return subFolders.Any(directory => directory.Name.Equals(dataFolderName));
    }

    public bool HasOldModLoaderInstalled()
    {
        var modLoaderDirectory =
            _fileSystem.DirectoryInfo.New(_fileSystem.Directory.GetCurrentDirectory());
        var vtolDirectory = modLoaderDirectory.Parent.Parent;

        if (_fileSystem.Directory.Exists(Path.Combine(vtolDirectory.FullName, "VTOLVR_ModLoader")))
        {
            return true;
        }

        return false;
    }
}