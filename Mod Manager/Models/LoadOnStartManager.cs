using Mod_Manager.Abstractions;
using Newtonsoft.Json;
using SteamQueries.Models;
using System.IO;
using System.IO.Abstractions;

namespace Mod_Manager.Models;

internal sealed class LoadOnStartManager : ILoadOnStartManager
{
    private readonly IFileManager _fileManager;
    public const string LoadOnStartFile = "loadonstart.json";

    public LoadOnStartManager(IFileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public LoadOnStart GetLoadOnStartItems()
    {
        var vtolPath = _fileManager.GetVtolDirectory();
        var nfilePath = Path.Combine(vtolPath, LoadOnStartFile);
        if (!File.Exists(nfilePath))
        {
            return new LoadOnStart();
        }
        
        var json = File.ReadAllText(nfilePath);
        
        // If, perchance, the file is empty.
        if (string.IsNullOrEmpty(json))
        {
            return new LoadOnStart();
        }
        
        return JsonConvert.DeserializeObject<LoadOnStart>(json);
    }

    
    public void ChangeStateOnItem(ulong itemId, bool newState)
    {
        var vtolPath = _fileManager.GetVtolDirectory();
        var nfilePath = Path.Combine(vtolPath, LoadOnStartFile);
        var settings = GetLoadOnStartItems();
        settings.WorkshopItems[itemId] = newState;
        var json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(nfilePath, json);
    }
    
    public void ChangeStateOnItem(string folderName, bool newState)
    {
        var vtolPath = _fileManager.GetVtolDirectory();
        var nfilePath = Path.Combine(vtolPath, LoadOnStartFile);
        var settings = GetLoadOnStartItems();
        settings.LocalItems[folderName] = newState;
        var json = JsonConvert.SerializeObject(settings);
        File.WriteAllText(nfilePath, json);
    }
}