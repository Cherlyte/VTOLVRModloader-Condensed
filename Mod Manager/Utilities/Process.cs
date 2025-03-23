using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mod_Manager.Abstractions;
using Mod_Manager.Models;

namespace Mod_Manager.Utilities;

internal class Process : IProcess
{
    private readonly ISettings _settings;
    private readonly IFileManager _fileManager;

    public Process(ISettings settings, IFileManager fileManager)
    {
        _settings = settings;
        _fileManager = fileManager;
    }

    public void Start(string fileName) => System.Diagnostics.Process.Start(fileName);
    public void Start(string fileName, string arguments) => System.Diagnostics.Process.Start(fileName, arguments);
    public void Start(ProcessStartInfo startInfo) => System.Diagnostics.Process.Start(startInfo);
    public System.Diagnostics.Process[] GetProcessesByName(string? processName) => System.Diagnostics.Process.GetProcessesByName(processName);
    
    public async Task<System.Diagnostics.Process?> StartVtolVr()
    {
        var args = new StringBuilder("--doorstop-enabled true");

        switch (_settings.GetVRMode())
        {
            case Settings.VRMode.SteamVR:
                break;
            case Settings.VRMode.Oculus:
                args.Append(" oculus");
                break;
            case Settings.VRMode.OpenXR:
                args.Append(" openxr");
                break;
        }

        var vtolPath = _fileManager.GetVtolDirectory();
        var finalPath = Path.Combine(vtolPath, "VTOLVR.exe");
        var processInfo = new ProcessStartInfo(finalPath, args.ToString());
        System.Diagnostics.Process.Start(processInfo);
        await Task.Delay(TimeSpan.FromSeconds(3));

        var vtolProcesses = GetProcessesByName("vtolvr");
        return !vtolProcesses.Any() ? null : vtolProcesses.First();
    }
}