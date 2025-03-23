namespace SteamQueries.Models
{
    public class SteamItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public string PreviewImageUrl { get; set; }
        public ulong NumSubscriptions { get; set; }
        public User Owner { get; set; }
        public ulong PublishFieldId { get; set; }
        public ItemMetaData MetaData { get; set; }
        public string Directory { get; set; }
        public ulong[] DependenciesIds { get; set; }
        public bool IsInstalled { get; set; }

        public override string ToString() =>
            $"{nameof(Title)}={Title}\n" +
            $"{nameof(Description)}={Description}\n" +
            $"{nameof(Tags)}={(Tags == null ? string.Empty : string.Join(",", Tags))}\n" +
            $"{nameof(PreviewImageUrl)}={PreviewImageUrl}\n" +
            $"{nameof(NumSubscriptions)}={NumSubscriptions}\n" +
            $"{nameof(Owner)}={Owner}\n" +
            $"{nameof(PublishFieldId)}={PublishFieldId}\n" +
            $"{nameof(MetaData)}={MetaData}\n" +
            $"{nameof(Directory)}={Directory}\n" +
            $"{nameof(DependenciesIds)}={(DependenciesIds == null ? string.Empty : string.Join(",", DependenciesIds))}\n" +
            $"{nameof(IsInstalled)}={IsInstalled}";
    }
}