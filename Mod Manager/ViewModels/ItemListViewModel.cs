using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Material.Icons;
using Mod_Manager.Abstractions;
using Mod_Manager.Abstractions.VIewModel;
using ReactiveUI;

namespace Mod_Manager.ViewModels;

internal sealed class ItemListViewModel : ViewModelBase, IViewModel
{
    public ReactiveCommand<Unit, Unit> OpenInSteam { get; }
    public ReactiveCommand<Unit, Unit> ToggleLoadOnStart { get; }
    public string Name { get; set; } = "[Workshop Item Name]";
    public ulong DownloadCount { get; set; } = 1234;
    public string Author { get; set; } = "[UserName]";
    private Bitmap? _image;
    public Bitmap? Image
    {
        get => _image;
        set => this.RaiseAndSetIfChanged(ref _image, value);
    }
    
    private string _losToolTip;
    public string LosToolTip
    {
        get => _losToolTip;
        set => this.RaiseAndSetIfChanged(ref _losToolTip, value); 
    }
    
    private MaterialIconKind _losIcon;
    public MaterialIconKind LosIcon
    {
        get => _losIcon;
        set => this.RaiseAndSetIfChanged(ref _losIcon, value); 
    }
    
    private bool _losAllowed;
    public bool LosAllowed
    {
        get => _losAllowed;
        set => this.RaiseAndSetIfChanged(ref _losAllowed, value); 
    }
    
    private bool _losEnabled;
    public bool LosEnabled
    {
        get => _losEnabled;
        set => this.RaiseAndSetIfChanged(ref _losEnabled, value); 
    }
    
    private bool _isSteamItem;
    public bool IsSteamItem
    {
        get => _isSteamItem;
        set => this.RaiseAndSetIfChanged(ref _isSteamItem, value); 
    }

    private readonly IHttp _http;
    private readonly ILoadOnStartManager _loadOnStartManager;
    private readonly string _previewImageUrl;
    private readonly string _workshopUrl;
    private readonly ulong _itemId;
    private readonly string _localItemFolderName;

    public ItemListViewModel()
    {
        OpenInSteam = ReactiveCommand.Create(OpenInSteamPressed, this.WhenAnyValue(x => x.IsSteamItem));
        ToggleLoadOnStart = ReactiveCommand.Create(ToggleLoadOnStartPressed);
        LosIcon = MaterialIconKind.Rocket;
        LosToolTip = "Enable Load on Start";
        LosAllowed = true;
        LosEnabled = false;
    }

    public ItemListViewModel(IHttp http,
        ILoadOnStartManager loadOnStartManager,
        ref string name,
        ulong downloadCount,
        string author,
        string previewImageUrl,
        string workshopUrl,
        bool losEnabled,
        bool losAllowed,
        ulong itemId) : this(http, loadOnStartManager, ref name, downloadCount, author, previewImageUrl, losEnabled, losAllowed)
    {
        IsSteamItem = true;
        _workshopUrl = workshopUrl;
        _itemId = itemId;
    }
    
    public ItemListViewModel(IHttp http,
        ILoadOnStartManager loadOnStartManager,
        ref string name,
        ulong downloadCount,
        string author,
        string previewImageUrl,
        bool losEnabled,
        bool losAllowed,
        string localItemFolderName) : this(http, loadOnStartManager, ref name, downloadCount, author, previewImageUrl, losEnabled, losAllowed)
    {
        IsSteamItem = false;
        _workshopUrl = string.Empty;
        _localItemFolderName = localItemFolderName;
    }

    private ItemListViewModel(IHttp http,
        ILoadOnStartManager loadOnStartManager,
        ref string name,
        ulong downloadCount,
        string author,
        string previewImageUrl,
        bool losEnabled,
        bool losAllowed) : this()
    {
        _previewImageUrl = previewImageUrl;
        Name = name;
        DownloadCount = downloadCount;
        Author = author;
        _http = http;
        _loadOnStartManager = loadOnStartManager;
        _losAllowed = losAllowed;
        SetLoadOnStartUi(losEnabled);
    }
    
    private void OpenInSteamPressed() => _http.OpenInSteam(_workshopUrl);

    private void ToggleLoadOnStartPressed()
    {
        var losEnabled = LosIcon == MaterialIconKind.RocketLaunch;
        if (_isSteamItem)
        {
            _loadOnStartManager.ChangeStateOnItem(_itemId, !losEnabled);
        }
        else
        {
            _loadOnStartManager.ChangeStateOnItem(_localItemFolderName, !losEnabled);
        }
        SetLoadOnStartUi(!losEnabled);
        LosEnabled = !losEnabled;
    }

    private void SetLoadOnStartUi(bool state)
    {
        if (!LosAllowed)
        {
            // This Tool Tip does not show. Waiting for Avalonia 11.1.x
            LosToolTip = "You can not enable Load on Start for this item";
            LosIcon = MaterialIconKind.Rocket;
            LosEnabled = false;
            return;
        }
        
        if (state)
        {
            LosToolTip = "Disable Load on Start";
            LosIcon = MaterialIconKind.RocketLaunch;
            LosEnabled = true;
            return;
        }
        LosToolTip = "Enable Load on Start";
        LosIcon = MaterialIconKind.Rocket;
        LosEnabled = false;
    }

    public async Task LoadImage()
    {
        Image = await _http.GetImageAsync(_previewImageUrl);
    }

    public ViewModelBase GetViewModel() => this;
}