using System.Diagnostics;
using System.Threading.Tasks;

namespace Mod_Manager.Abstractions;

public interface IProcess
{
    void Start(string fileName);
    void Start(string fileName, string arguments);
    void Start(ProcessStartInfo startInfo);
    Process[] GetProcessesByName(string? processName);
    Task<Process?> StartVtolVr();
}