namespace SteamQueries.Models
{
    public class ItemMetaData
    {
        public string DllName { get; set; }
        public bool AllowLoadOnStart { get; set; }
        public bool ShowOnMainList { get; set; }
        public string DllHash { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as ItemMetaData;
            
            if (item == null)
                return false;

            return Equals(item.DllName, DllName) &&
                   Equals(item.AllowLoadOnStart, AllowLoadOnStart) &&
                   Equals(item.ShowOnMainList, ShowOnMainList) &&
                   Equals(item.DllHash, DllHash);
        }

        public override int GetHashCode()
        {
            return DllName.GetHashCode() + 
                   AllowLoadOnStart.GetHashCode() + 
                   ShowOnMainList.GetHashCode() +
                   DllHash.GetHashCode();
        }
    }
}