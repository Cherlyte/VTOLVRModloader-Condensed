using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using ModLoader.Framework.DLC;
using ModLoader.Framework.Exceptions;
using SteamQueries.Models;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace ModLoader;

public class ModLoader : MonoBehaviour
{
    public static ModLoader Instance { get; private set; }

    public IReadOnlyCollection<LoadedItem> LoadedItems
    {
        get
        {
            return _loadedItems.Values;
        }
    }

    private readonly Dictionary<string, LoadedItem> _loadedItems = new ();
    private const string _localItemsFolderName = @"@Mod Loader\Mods";
    private const string _localSteamItemFile = "item.json";
        
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    internal UniTask<bool> LoadSteamItem(SteamItem item)
    {
        if (IsItemLoaded(item.Directory))
        {
            Debug.LogWarning($"'{item.Title}' is already loaded");
            return new UniTask<bool>(true);
        }

        return LoadMod(item);
    }

    /// <summary>
    /// Destroys any game objects of the passed in item and if a dependency isn't required by anything else it will also destroy its game objects
    /// </summary>
    /// <param name="item"></param>
    internal async UniTask DisableSteamItem(SteamItem item)
    {
        if (!IsItemLoaded(item.Directory))
        {
            Debug.LogWarning($"'{item.Title} isn't loaded");
            return;
        }

        await RemoveItemGameObjects(item);
    }

    private async UniTask RemoveItemGameObjects(SteamItem item)
    {
        Debug.Log($"Disabling '{item.Title}'");
        var assembly = GetSteamItemAssembly(item);
        if (ContainsVtolMod(assembly, out var types))
        {
            foreach (Type type in types)
            {
                var typeGameObject = GameObject.FindObjectOfType(type, true);
                if (typeGameObject == null)
                {
                    Debug.LogError($"Couldn't find any game objects of type '{type.FullName}'");
                    continue;
                }
                VtolMod mod = typeGameObject as VtolMod;
                if (mod == null)
                {
                    Debug.LogError($"Couldn't convert '{type.Name}' to {nameof(VtolMod)}");
                    continue;
                }
                mod.UnLoad(); // Coroutines won't have finished but oh well
                Destroy(mod.gameObject);
            }
        }
        else
        {
            Debug.Log($"There was no VTOL mods for '{item.Title}' so didn't destroy anything related to it");
        }

        if (_loadedItems.TryGetValue(item.Directory, out var loadedItem))
        {
            loadedItem.Harmony.UnpatchSelf();
            _loadedItems.Remove(item.Directory);
        }

        RemoveDirectoryFromResolve(item);

        if (item.DependenciesIds == null || !item.DependenciesIds.Any())
        {
            return;
        }

        Debug.Log($"Disabling Dependencies of '{item.Title}'");
    }

    internal bool IsItemLoaded(string directory) => _loadedItems.ContainsKey(directory);

    private async UniTask<bool> LoadMod(SteamItem item, ulong itemReasonId = 0)
    {

        if (item.PublishFieldId > 0 && _loadedItems.Values.Any(i => i.Item.PublishFieldId == item.PublishFieldId))
            return true;
            
        Debug.Log("Getting local items");
        var localItems = FindLocalItems();
        
        Debug.Log($"{item.Title} dependencies ids {(item.DependenciesIds == null ? "is null" : (item.DependenciesIds.Any() ? $"has a count of {item.DependenciesIds.Length}" : "is not null but empty"))}");
        if (item.DependenciesIds != null && item.DependenciesIds.Any())
        {
            Debug.Log($"Loading Dependencies of '{item.Title}'");
                
            foreach (var publishFieldId in item.DependenciesIds)
            { 
                // Load local dependencies if they exist.
                bool loadedLocalItem = false;
                foreach (var localItem in localItems.Where(i => i.PublishFieldId == publishFieldId))
                {
                    if (!await LoadMod(localItem, item.PublishFieldId))
                        break;
                    loadedLocalItem = true;
                }
                    
                if (loadedLocalItem)
                    continue;

            }
        }
            
        Debug.Log($"Loading '{item.Title}'");
            
        try
        {
            AddDirectoryToResolve(item);

            var assembly = string.IsNullOrEmpty(item.MetaData.DllName) ? null : LoadAssembly(item);
                
            if (!_loadedItems.TryGetValue(item.Directory, out var record))
            {
                record = new LoadedItem();
                if (ContainsVtolMod(assembly, out var types))
                {
                    var validTypes = types.Where(t => UserHasCorrectDLC(t, item)).ToArray();
                    if (validTypes.Length == 0)
                    {
                        Debug.LogError($"Failed to load {item.Title} due to not having the required DLCs");
                        return false;
                    }
                    
                    foreach (var type in validTypes)
                    {
                        SpawnVtolMod(type, assembly, item, out var harmony);
                        record.Harmony = harmony;
                    }
                }

                var list = new List<ulong>();
                if (itemReasonId != 0)
                {
                    list.Add(itemReasonId);
                }

                record.ItemsThatDependOnThis = list;
                record.Item = item;
                    
                _loadedItems.Add(
                    item.Directory,
                    record);
            }
            else
            {
                if (!record.ItemsThatDependOnThis.Contains(itemReasonId))
                {
                    record.ItemsThatDependOnThis.Add(itemReasonId);
                }
            }
                
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load '{item.Title}'");
            Debug.LogError(e);
            return false;
        }
    }

    private static void AddDirectoryToResolve(SteamItem item)
    {
        var searchDirectories = Environment.GetEnvironmentVariable("DOORSTOP_DLL_SEARCH_DIRS");
        var split = searchDirectories.Split(';').ToList();
        split.Insert(1, item.Directory);
        Environment.SetEnvironmentVariable("DOORSTOP_DLL_SEARCH_DIRS", string.Join(";", split));
        Debug.Log($"DOORSTOP_DLL_SEARCH_DIRS={Environment.GetEnvironmentVariable("DOORSTOP_DLL_SEARCH_DIRS")}");
    }

    private static void RemoveDirectoryFromResolve(SteamItem item)
    {
        var searchDirectories = Environment.GetEnvironmentVariable("DOORSTOP_DLL_SEARCH_DIRS");
        var split = searchDirectories.Split(';').ToList();
        if (split.Contains(item.Directory))
        {
            split.Remove(item.Directory);
        }
        Environment.SetEnvironmentVariable("DOORSTOP_DLL_SEARCH_DIRS", string.Join(";", split));
        Debug.Log($"DOORSTOP_DLL_SEARCH_DIRS={Environment.GetEnvironmentVariable("DOORSTOP_DLL_SEARCH_DIRS")}");
    }

    private Assembly LoadAssembly(SteamItem item)
    {
        var modPath = GetSteamItemDllPath(item);
        return CheckIfAssemblyIsLoaded(modPath, out var assembly) ? assembly : Assembly.LoadFrom(modPath);
    }

    private string GetSteamItemDllPath(SteamItem item) => Path.Combine(item.Directory, item.MetaData.DllName);

    private bool CheckIfAssemblyIsLoaded(string fullPath, out Assembly assembly)
    {
        var currentDomain = AppDomain.CurrentDomain;
        var currentAssemblies = currentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic);

        assembly = currentAssemblies.FirstOrDefault(assembly => assembly.Location.Equals(fullPath));

        return assembly != null;
    }

    private bool ContainsVtolMod(Assembly assembly, out IEnumerable<Type> types)
    {
        types = [];
        
        if (assembly == null)
        {
            return false;
        }
        
        types = from t in assembly.GetTypes()
            where t.IsSubclassOf(typeof(VtolMod))
            select t;

        return types.Any();
    }

    private void SpawnVtolMod(Type itemType, Assembly assembly, SteamItem item, out HarmonyLib.Harmony harmony)
    {
        harmony = null;
        var id = (ItemId)Attribute.GetCustomAttribute(itemType, typeof(ItemId)) ??
                 throw new ItemIdMissingException($"Missing '{nameof(ItemId)}' attribute on '{itemType.Name}'");

        if (!HarmonyLib.Harmony.HasAnyPatches(id.UniqueValue))
        {
            harmony = new HarmonyLib.Harmony(id.UniqueValue);
            try
            {
                harmony.PatchAll(assembly);
            }
            catch (Exception e)
            {
                Debug.LogError("FAILED TO APPLY PATCHES");
                Debug.LogError(e);
            }
            Debug.Log($"Harmony Patched Assembly '{assembly.FullName}' with id of '{id.UniqueValue}'");
        }
        else
        {
            Debug.Log($"Assembly '{assembly.FullName}' was already patched!");
        }
            
            

        var itemGameObject = new GameObject(itemType.FullName, itemType);
        DontDestroyOnLoad(itemGameObject);

        Debug.Log($"Created GameObject '{itemGameObject.name}' from type '{itemType.Name}'");
    }

    // ReSharper disable once InconsistentNaming
    private bool UserHasCorrectDLC(Type itemType, SteamItem item)
    {
        var attribute = (RequiredDLC)Attribute.GetCustomAttribute(itemType, typeof(RequiredDLC));
        if (attribute == null)
        {
            return true;
        }

        // ReSharper disable once InconsistentNaming
        var requiredDLC = attribute.DLC;

        if (requiredDLC.HasFlag(VTOLVRDLC.AH94))
        {
            if (File.Exists(Path.Combine("DLC", "1770480", "1770480")))
            {
                Debug.Log("VTOL VR: AH-94 Attack Helicopter is installed");
            }
            else
            {
                Debug.LogError($"VTOL VR: AH-94 Attack Helicopter is not installed but is required for '{item.Title}'");
                return false;
            }
            
        }

        if (requiredDLC.HasFlag(VTOLVRDLC.T55))
        {
            if(File.Exists(Path.Combine("DLC", "2141700", "T-55License.txt")))
            {
                Debug.Log("VTOL VR: T-55 Tyro - Trainer Jet is installed");
            }
            else
            {
                Debug.LogError($"VTOL VR: T-55 Tyro - Trainer Jet is not installed but is required for '{item.Title}'");
                return false;
            }
        }

        if (requiredDLC.HasFlag(VTOLVRDLC.EF24G))
        {
            if (File.Exists(Path.Combine("DLC", "2531290", "2531290")))
            {
                Debug.Log("VTOL VR: EF-24 Mischief - Electronic Warfare is installed");
            }
            else
            {
                Debug.LogError($"VTOL VR: EF-24 Mischief - Electronic Warfare is not installed but is required for '{item.Title}'");
                return false;
            }
        }

        return true;
    }

    [CanBeNull]
    private Assembly GetSteamItemAssembly(SteamItem item)
    {
        var path = GetSteamItemDllPath(item);

        if (!CheckIfAssemblyIsLoaded(path, out var assembly))
        {
            Debug.LogError("Attempted to unload");
            return null;
        }

        return assembly;
    }
        
    public IReadOnlyCollection<SteamItem> FindLocalItems()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), _localItemsFolderName);
        var folder = new DirectoryInfo(path);
        if (!folder.Exists)
        {
            Debug.Log($"No local items due to '{path}' not existing");
            return Array.Empty<SteamItem>();
        }

        var subDirectories = folder.GetDirectories();

        if (!subDirectories.Any())
        {
            Debug.Log("Skipping local items due to no sub directories found");
            return Array.Empty<SteamItem>();
        }

        var returnValue = new List<SteamItem>();

        foreach (var directory in subDirectories)
        {
            var localItemFile = Path.Combine(directory.FullName, _localSteamItemFile);
            if (!File.Exists(localItemFile))
            {
                Debug.Log($"{directory.Name} did not have {_localSteamItemFile}");
                continue;
            }

            try
            {
                var text = File.ReadAllText(localItemFile);
                var jsonObject = JsonConvert.DeserializeObject<SteamItem>(text);
                jsonObject.IsInstalled = true;
                jsonObject.Directory = directory.FullName;
                returnValue.Add(jsonObject);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read JSON from '{path}'\n{e.Message}");
                continue;
            }
        }

        return returnValue;
    }
}