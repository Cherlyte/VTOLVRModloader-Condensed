using System;
using System.Runtime.InteropServices;

namespace Mod_Manager.Abstractions;

public interface IRuntimeInfo
{
    bool IsOSPlatform(OSPlatform osPlatform);
    string GetFolderPath(Environment.SpecialFolder folder);
}