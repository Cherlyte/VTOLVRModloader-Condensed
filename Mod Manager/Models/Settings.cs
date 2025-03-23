using Mod_Manager.Abstractions;
using Mod_Manager.Abstractions.ConfigParser;
using Newtonsoft.Json;
using System.IO;

namespace Mod_Manager.Models;

internal sealed class Settings: ISettings
{
    private readonly IConfigParser _configParser;
    private readonly IFileManager _fileManager;
    private const string _fileName = "settings.json";
    private const string _dfileName = "doorstop_config.ini";

    public Settings(IFileManager fileManager, IConfigParser configParser)
    {
        _fileManager = fileManager;
        _configParser = configParser;
    }

    public VRMode GetVRMode()
    {
        var settings = LoadSettings();
        return settings.VRMode;
    }
    public void SetVRMode(VRMode newValue)
    {
        var settings = LoadSettings();
        settings.VRMode = newValue;
        SaveSettings(settings);
    }

    public bool GetDoorstopE()
    {
        var vPath = _fileManager.GetVtolDirectory();
        var npath = Path.Combine(vPath, _dfileName);
        _configParser.SetFile(npath);
        return _configParser.GetValue("General", "enabled", true);
    }
    public void SetDoorstopE(bool newValue)
    {
        var settings = LoadSettings();
        var vPath = _fileManager.GetVtolDirectory();
        var npath = Path.Combine(vPath, _dfileName);
        _configParser.SetFile(npath);
        _configParser.SetValue("General", "enabled", newValue);
        settings.DoorstopE = newValue;
        SaveSettings(settings);
        _configParser.Save();
    } 

    private SettingsJson LoadSettings()
    {
        var vtolF = _fileManager.GetVtolDirectory();
        var nPath = Path.Combine(vtolF, _fileName);
        if (!File.Exists(nPath))
        {
            return new SettingsJson();
        }

        var fileText = File.ReadAllText(nPath);
        
        // Somehow Steam is saying the file exists but when reading the bytes its null, throwing
        if (string.IsNullOrWhiteSpace(fileText))
        {
            return new SettingsJson();
        }
        
        var convertedObject = JsonConvert.DeserializeObject<SettingsJson>(fileText);
        return convertedObject;
    }

    private void SaveSettings(SettingsJson settingsClass)
    {
        var vtolF = _fileManager.GetVtolDirectory();
        var nPath = Path.Combine(vtolF, _fileName);
        var jsonText = JsonConvert.SerializeObject(settingsClass, Formatting.Indented);
        File.WriteAllText(nPath, jsonText);
    }
    
    public enum VRMode
    {
        SteamVR,
        Oculus,
        OpenXR
    }

    private class SettingsJson
    {
        [JsonProperty]
        public VRMode VRMode;
        public bool DoorstopE;
    }
}