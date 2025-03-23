using System.Threading.Tasks;

namespace Mod_Manager.Abstractions;

public interface ILogsZipGenerator
{
    /// <returns>Full path to the resulting zip</returns>
    Task<string> CollectLogs();
}