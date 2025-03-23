using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Mod_Manager.Abstractions;
using Mod_Manager.Abstractions.VIewModel;
using Mod_Manager.Models;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using SteamQueries.Models;

namespace Mod_Manager.ViewModels;

internal sealed class HomeViewModel : ViewModelBase, IHomeViewModel
{
    private ObservableCollection<ItemListViewModel> _myItems = new ();
    public ObservableCollection<ItemListViewModel> MyItems
    {
        get => _myItems;
        set => this.RaiseAndSetIfChanged(ref _myItems, value); 
    }

    private const string _localItemsFolderName = @"@Mod Loader\Mods";
    private const string _localItemFile = "item.json";
    private readonly IHttp _http;
    private readonly ILoadOnStartManager _loadOnStartManager;
    private readonly IFileSystem _fileSystem;

    public HomeViewModel(IHttp http, ILoadOnStartManager loadOnStartManager, IFileSystem fileSystem) : this()
    {
        _http = http;
        _loadOnStartManager = loadOnStartManager;
        _fileSystem = fileSystem;
    }
    
    public HomeViewModel()
    {
        
    }

    public void ClearList() => MyItems.Clear();

    public void GetLocalItems(string baseGameFolder)
    {
        var folder = _fileSystem.DirectoryInfo.New(Path.Combine(baseGameFolder, _localItemsFolderName));
        if (!folder.Exists)
        {
            Log.Information("Could not find local items at '{LocalItemsFolder}'", folder);
            return;
        }

        var subFolder = folder.GetDirectories();

        if (!subFolder.Any())
        {
            Log.Information("There was no sub directories in {LocalItemsFolder}", folder.FullName);
            Log.Information("If you where expecting your item to show, place it inside a sub folder");
            return;
        }

        foreach (var directoryInfo in subFolder)
        {
            var itemDataPath = Path.Combine(directoryInfo.FullName, _localItemFile);
            if (!_fileSystem.File.Exists(itemDataPath))
            {
                Log.Warning("'{ExpectedFileName}' was not found in the folder '{FolderName}'", itemDataPath, directoryInfo.Name);
                Log.Information("If you where expecting your item to show up, please create this file and fill out the JSON properties");
                continue;
            }
            
            var itemData = default(SteamItem);
            try
            {
                itemData = JsonConvert.DeserializeObject<SteamItem>(_fileSystem.File.ReadAllText(itemDataPath));
            }
            catch (Exception e)
            {
                Log.Error("Error when reading items json '{ErrorMessage}'. Item will not show", e.Message);
                continue;
            }
            
            var losEnabled = false;
            if (_loadOnStartManager.GetLoadOnStartItems().LocalItems.TryGetValue(directoryInfo.Name, out bool workshopItem))
            {
                losEnabled = workshopItem;
            }
            
            Log.Information("Found item '{ItemName}, {ItemAuthor}, {DllName}'", itemData.Title, itemData.Owner == null ? "You" : itemData.Owner.Name, itemData.MetaData?.DllName);
            
            var name = itemData.Title;
            MyItems.Add(new ItemListViewModel(_http, 
                _loadOnStartManager,
                ref name,
                0,
                itemData.Owner == null ? "You" : itemData.Owner.Name,
                itemData.PreviewImageUrl,
                losEnabled,
                itemData.MetaData?.AllowLoadOnStart ?? true,
                directoryInfo.Name));
        }
    }

    public async Task DownloadImages()
    {
        foreach (var item in MyItems)
        {
            await item.LoadImage();
        }
    }

    public ViewModelBase GetViewModel() => this;
}