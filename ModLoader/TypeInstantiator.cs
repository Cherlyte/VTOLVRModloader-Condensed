using System;
using System.Collections.Generic;
using Mod_Loader.Classes;
using ModLoader.Assets;
using ModLoader.Assets.ReadyRoom;
using ModLoader.IntegratedMods;
using Serilog;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModLoader
{
    internal class TypeInstantiator : MonoBehaviour
    {
        private readonly Dictionary<VTOLScenes, Type[]> _types = new()
        {
            { VTOLScenes.SplashScene, new []{ typeof(AssetBundleLoader), typeof(ModLoader) }},
            { VTOLScenes.SamplerScene, new []{ typeof(LoadOnStartSpawner) }},
            { VTOLScenes.ReadyRoom , new []{  typeof(ModsPage), typeof(SettingsList) }}
        };
        
        private void Awake()
        {
            PlayerLogText();
            Debug.Log("Hello From Type Instantiator");
            SceneManager.sceneLoaded += OnSceneLoaded;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
            // This object gets created on the splash scene, so the event does 
            // not get fired. So we manually trigger it.
            OnSceneLoaded(VTOLScenes.SplashScene);
            DontDestroyOnLoad(this);
        }

        private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Exception:
                    Log.Error("{Message}\n{StackTrace}", condition, stacktrace);
                    break;
                case LogType.Error:
                    Log.Error("{Message}", condition);
                    break;
                case LogType.Assert:
                case LogType.Warning:
                    Log.Warning("{Message}", condition);
                    break;
                case LogType.Log:
                    Log.Information("{Message}", condition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            var scene = (VTOLScenes)arg0.buildIndex;
            OnSceneLoaded(scene);
        }

        private void OnSceneLoaded(VTOLScenes scene)
        {
            if (!_types.TryGetValue(scene, out var objectsToSpawn))
            {
                return;
            }

            foreach (var monoBehaviour in objectsToSpawn)
            {
                new GameObject(monoBehaviour.Name, monoBehaviour);
                Debug.Log($"Created Type '{monoBehaviour.FullName}'");
            } 
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private static void PlayerLogText()
        {
            string playerLogMessage = @" 
                                                                                                         
                                                                                                         
 #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  ##### 
                                                                                                         
 #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  ##### 
                                                                                                         
 #     #                                              #     #                                            
 ##   ##   ####   #####   #####   ######  #####       #     #  ######  #####    ####   #   ####   #    # 
 # # # #  #    #  #    #  #    #  #       #    #      #     #  #       #    #  #       #  #    #  ##   # 
 #  #  #  #    #  #    #  #    #  #####   #    #      #     #  #####   #    #   ####   #  #    #  # #  # 
 #     #  #    #  #    #  #    #  #       #    #       #   #   #       #####        #  #  #    #  #  # # 
 #     #  #    #  #    #  #    #  #       #    #        # #    #       #   #   #    #  #  #    #  #   ## 
 #     #   ####   #####   #####   ######  #####          #     ######  #    #   ####   #   ####   #    # 

Thank you for downloading VTOL VR Mod loader by . Marsh.Mello .

Please don't report bugs unless you can reproduce them without any mods loaded
if you are having any issues with mods and would like to report a bug, please contact @. Marsh.Mello .#0001 
on the offical VTOL VR Discord or post an issue on gitlab. 

 #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  ##### 
                                                                                                         
 #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  #####  ##### 
";
            UnityEngine.Debug.Log(playerLogMessage);
        }
    }
}