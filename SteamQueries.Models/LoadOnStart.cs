using System.Collections.Generic;

namespace SteamQueries.Models
{
    public sealed class LoadOnStart
    {
        public Dictionary<ulong, bool> WorkshopItems { get; set; } = new Dictionary<ulong, bool>();
        public Dictionary<string, bool> LocalItems { get; set; } = new Dictionary<string, bool>();
    }
}