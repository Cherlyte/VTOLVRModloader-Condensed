using System;
using System.IO;
using ModLoader.Framework.Attributes;
using Tomlet;
using UnityEngine;

namespace ModLoader.Framework.Settings;

public abstract class BaseSettings
{
    private const string _settingsDirectory = @"@Mod Loader\Settings";
    private VtolMod _mod;

    public static T New<T>(VtolMod mod) where T : BaseSettings
    {
        var filePath = GetFilePath<T>(mod);

        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("Failed to find the file path, returning default settings");
            return Activator.CreateInstance<T>();
        }
        
        var directory = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(directory);

        if (!File.Exists(filePath))
        {
            var instance = Activator.CreateInstance<T>();
            instance._mod = mod;
            return instance;
        }

        var tomlDocument = TomlParser.ParseFile(filePath);
        var returnValue = (T)TomletMain.To(typeof(T), tomlDocument);
        returnValue._mod = mod;
        return returnValue;
    }

    public void Save()
    {
        var text = TomletMain.TomlStringFrom(GetType(), this);
        var filePath = GetFilePath(GetType(), _mod);
        File.WriteAllText(filePath, text);
    }

    private static string GetFilePath<T>(VtolMod mod) where T : BaseSettings => GetFilePath(typeof(T), mod);

    private static string GetFilePath(Type type, VtolMod mod)
    {
        if (mod == null)
        {
            Debug.LogError($"{nameof(mod)} cannot be null.");
            return string.Empty;
        }
        
        var attribute = (ItemId)Attribute.GetCustomAttribute(mod.GetType(), typeof(ItemId));
        if (attribute == null)
        {
            Debug.LogError($"Failed to fetch {nameof(ItemId)} off the mod.");
            return string.Empty;
        }

        return Path.Combine(_settingsDirectory, attribute.UniqueValue, $"{type.Name}.toml");
    }
}