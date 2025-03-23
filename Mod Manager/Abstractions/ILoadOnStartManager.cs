using SteamQueries.Models;

namespace Mod_Manager.Abstractions;

internal interface ILoadOnStartManager
{
    LoadOnStart GetLoadOnStartItems();
    
    /// <summary>
    /// Changing load on start for workshop items
    /// </summary>
    /// <param name="itemId">The Steam Id of this item</param>
    /// <param name="newState"></param>
    void ChangeStateOnItem(ulong itemId, bool newState);

    /// <summary>
    /// Changing load on start for local items
    /// </summary>
    /// <param name="folderName">The name of the folder which the mod is in, not the full path.</param>
    /// <param name="newState"></param>
    void ChangeStateOnItem(string folderName, bool newState);
}