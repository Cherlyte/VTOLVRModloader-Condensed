using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ModLoader.Assets
{
    internal class AssetBundleLoader : MonoBehaviour
    {
        private const string _assetBundleName = "assets";
        private const string _modLoaderFolder = "@Mod Loader";
        private const string _unityProjectRootPath = "Assets/@Mod Loader";
        private const string _unityProjectWarningTextPath = _unityProjectRootPath + "/SplashScene/Modded Warning Text.prefab";

        private const string _disableModLoaderButtonPrefab =
            _unityProjectRootPath + "/SamplerScene/Disable Mod Loader Button.prefab";

        public static AssetBundleLoader Instance { get; private set; }

        private Dictionary<string, GameObject> _prefabs = new();
        private AssetBundle _assetBundle;

        private void Awake()
        {
            Debug.Log("I'M HERE I'M HERE!!!!");
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        public IEnumerator LoadAssets(AsyncOpStatus status)
        {
            var path = Path.Combine(_modLoaderFolder, _assetBundleName);
            var request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
            {
                status.progress = request.progress;
                yield return null;
            }
            
            _assetBundle = request.assetBundle;
            if (_assetBundle == null)
            {
                Debug.LogError($"Failed to load Mod Loader's asset bundle from {path}");
                yield break;
            }

            var allAssetsRequest = _assetBundle.LoadAllAssetsAsync<GameObject>();
            while (!allAssetsRequest.isDone)
            {
                status.progress = request.progress;
                yield return null;
            }

            var prefabs = allAssetsRequest.allAssets;
            
            foreach (GameObject prefab in prefabs)
            {
                _prefabs.Add(prefab.name, prefab);
            }
        }
        public GameObject SpawnPrefab(string prefabName, float posX = 0, float posY = 0, float posZ = 0,
            float rotX = 0, float rotY = 0, float rotZ = 0)
        {
            if (!_prefabs.TryGetValue(prefabName, out GameObject prefab))
            {
                throw new Exception($"Couldn't find prefab called '{prefabName}'");
            }

            var position = new Vector3(posX, posY, posZ);
            var rotation = Quaternion.Euler(rotX, rotY, rotZ);
            Debug.Log($"Spawning Prefab '{prefabName}' at {position} with rotation of {rotation}");
            
            var instance = Instantiate(prefab);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            return instance;
        }
    }
}