using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Mod_Manager.Abstractions;

internal interface IInit
{
    bool IsInGameFiles();
    bool IsVtolFolder(IDirectoryInfo folder);
    bool HasOldModLoaderInstalled();
}