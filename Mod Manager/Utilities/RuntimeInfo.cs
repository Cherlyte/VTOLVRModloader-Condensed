using System;
using System.Runtime.InteropServices;
using Mod_Manager.Abstractions;

namespace Mod_Manager.Utilities;

public class RuntimeInfo : IRuntimeInfo
{
    public bool IsOSPlatform(OSPlatform osPlatform) =>
        RuntimeInformation.IsOSPlatform(osPlatform);

    public string GetFolderPath(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);
}