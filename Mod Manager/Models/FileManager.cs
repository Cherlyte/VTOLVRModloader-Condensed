using System.IO.Abstractions;
using Mod_Manager.Abstractions;

namespace Mod_Manager.Models;

internal sealed class FileManager : IFileManager
{
    
    private readonly IFileSystem _fileSystem;

    public FileManager(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string GetVtolDirectory()
    {
        var currentDirectory = _fileSystem.Directory.GetCurrentDirectory();
        var vtolDirectory = _fileSystem.DirectoryInfo.New(currentDirectory).Parent.Parent;
        return vtolDirectory.FullName;
    }
}