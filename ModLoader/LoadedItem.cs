using System.Collections.Generic;
using SteamQueries.Models;

namespace ModLoader;

public record LoadedItem
{
    public SteamItem Item { get; set; }
    public List<ulong> ItemsThatDependOnThis { get; set; }
    public HarmonyLib.Harmony Harmony { get; set; }
}