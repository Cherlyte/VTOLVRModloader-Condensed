using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using SteamQueries.Models;
using Serilog;
using UnityEngine;
using Newtonsoft.Json;

namespace ModLoader
{
    internal class LoadOnStartSpawner : MonoBehaviour
    {
        private async UniTaskVoid Start()
        {
            Debug.Log($"Hello from {nameof(LoadOnStartSpawner)}. Cher was here!");        
            await RequestForLoadOnStartSettings();
        }

        private async UniTask RequestForLoadOnStartSettings()
        {
            var LoSPath = @"loadonstart.json";
            var response = File.ReadAllText(LoSPath);
            var settings = JsonConvert.DeserializeObject<LoadOnStart>(response);

            if (settings == null || !File.Exists(LoSPath))
            {
                Debug.LogError("Load on Start Settings are null, defaulting to None.");
                return;
            }
            Debug.Log(File.ReadAllText(LoSPath));
            Debug.Log($"Oioi! {settings.LocalItems}");
            await LoadLocalItems(settings.LocalItems);
        }

        private async UniTask LoadLocalItems(Dictionary<string, bool> localItems)
        {
            var itemsToLoad = localItems.Where(item => item.Value)
                .ToDictionary(item => item.Key, item => item.Value);
            if (!itemsToLoad.Any())
            {
                Log.Information("Load On Start has no local items enabled");
                return;
            }

            var localItemsFound = ModLoader.Instance.FindLocalItems();

            foreach (var item in localItemsFound)
            {
                var name = Path.GetFileName(item.Directory);
                if (itemsToLoad.ContainsKey(name))
                {
                    await ModLoader.Instance.LoadSteamItem(item);
                }
            }
        }
    }
}