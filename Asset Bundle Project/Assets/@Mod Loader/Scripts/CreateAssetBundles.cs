#if UNITY_EDITOR 
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("@Mod Loader/Create Manual Asset Bundle")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/@Mod Loader/Output";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows64);;
    }
}

#endif