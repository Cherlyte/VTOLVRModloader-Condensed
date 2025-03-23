using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Mod_Manager.Abstractions;
using Mod_Manager.Abstractions.VIewModel;
using Mod_Manager.Models;
using ReactiveUI;

namespace Mod_Manager.ViewModels;
sealed class HeaderBarViewModel : ViewModelBase, IHeaderBarViewModel
{
    private readonly ILogsZipGenerator _logsZipGenerator;
    private readonly IRuntimeInfo _runtimeInfo;
    private readonly IProcess _process;
    private readonly IFileManager _fileManager;
    private readonly IHttp _http;
    private readonly IPopUpView _popUpView;
    private readonly IHomeViewModel _homeViewModel;
    private readonly IFileSystem _fileSystem;
    private readonly ISettings _settings;
    private readonly IInit _init;
    public ReactiveCommand<Unit, Unit> HelpCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenModsFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> LaunchCommand { get; }
    public ReactiveCommand<Unit, Unit> CollectLogsCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    public ReactiveCommand<Unit, Unit> UseOculusCommand { get; }
    public ReactiveCommand<Unit, Unit> UseSteamVRCommand { get; }
    public ReactiveCommand<Unit, Unit> UseOpenXRCommand { get; }

    public ReactiveCommand<Unit, Unit> DoorstopEnablerCommand { get; }
    
    private string _toggleButtonText = "Play";
    public string ToggleButtonText
    {
        get => _toggleButtonText;
        set => this.RaiseAndSetIfChanged(ref _toggleButtonText, value); 
    }
    
    private bool _canLaunchGame = true;
    public bool CanLaunchGame
    {
        get => _canLaunchGame;
        set => this.RaiseAndSetIfChanged(ref _canLaunchGame, value); 
    }

    public bool UseOculus { get; private set; }
    public bool UseSteamVR { get; private set; }
    public bool UseOpenXR { get; private set; }
    public bool DoorstopEnabled { get; private set; }

    public HeaderBarViewModel()
    {
        HelpCommand = ReactiveCommand.Create(HelpButtonPressed);
        OpenModsFolderCommand = ReactiveCommand.Create(OpenModsFolderPressed);
        LaunchCommand = ReactiveCommand.CreateFromTask(LaunchButtonPressed);
        CollectLogsCommand = ReactiveCommand.CreateFromTask(CreateLogsPressed);
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshPressed);
        UseOculusCommand = ReactiveCommand.Create(UseOculusPressed);
        UseSteamVRCommand = ReactiveCommand.Create(UseSteamVRPressed);
        UseOpenXRCommand = ReactiveCommand.Create(UseOpenXRPressed);
        DoorstopEnablerCommand = ReactiveCommand.Create(DoorstopModeSwitch);
    }

    public HeaderBarViewModel(ILogsZipGenerator logsZipGenerator,
        IRuntimeInfo runtimeInfo,
        IFileManager fileManager,
        IProcess process,
        IHttp http,
        IPopUpView popUpView,
        IHomeViewModel homeViewModel,
        IFileSystem fileSystem,
        ISettings settings,
        IInit init) : this()
    {
        _logsZipGenerator = logsZipGenerator;
        _runtimeInfo = runtimeInfo;
        _process = process;
        _http = http;
        _fileManager = fileManager;
        _popUpView = popUpView;
        _homeViewModel = homeViewModel;
        _fileSystem = fileSystem;
        _settings = settings;
        _init = init;

        switch (_settings.GetVRMode())
        {
            case Settings.VRMode.SteamVR:
                UseSteamVR = true;
                break;
            case Settings.VRMode.Oculus:
                UseOculus = true;
                break;
            case Settings.VRMode.OpenXR:
                UseOpenXR = true;
                break;
        }
        switch (_settings.GetDoorstopE())
        {
            case true:
                DoorstopEnabled = true;
                break;
            case false:
                DoorstopEnabled = false;
                break;
        }
    }

    private void OpenModsFolderPressed()
    {
        var vPath = _fileManager.GetVtolDirectory();
        var fpath = Path.Combine(vPath, @"@Mod Loader\Mods");
        if (string.IsNullOrEmpty(fpath))
        {
            _popUpView.ShowPopUp("Failed", "Failed to find the path to the Mods folder.", rightButtonText:"Aww...");
            return;
        }
        
        if (_runtimeInfo.IsOSPlatform(OSPlatform.Windows))
        {
            _process.Start("explorer.exe", fpath);
            return;
        }
        _popUpView.ShowPopUp("Expected folder path", fpath, rightButtonText: "Okay");
    }

    private async Task LaunchButtonPressed()
    {
        ToggleButtonText = "Launching...";
        CanLaunchGame = false;
        var vtolVr = await _process.StartVtolVr();
        if (vtolVr == null)
        {
            CanLaunchGame = true;
            ToggleButtonText = "Play";
            return;
        }
        
        ToggleButtonText = "Running";

        vtolVr.EnableRaisingEvents = true;
        vtolVr.Exited += (_, _) =>
        {
            CanLaunchGame = true;
            ToggleButtonText = "Play";
        };
    }

    private void HelpButtonPressed() => _http.OpenUrl("https://vtolvr-mods.com/docs/getting-started/");
    
    private async Task CreateLogsPressed()
    {
        var vtolProcesses = _process.GetProcessesByName("vtolvr");
        if (vtolProcesses.Any())
        {
            await _popUpView.ShowPopUp("Failed", "Please close VTOL VR before trying to collect logs", rightButtonText: "Okay");
            return;
        }
        
        var zipPath = await _logsZipGenerator.CollectLogs();
        if (_runtimeInfo.IsOSPlatform(OSPlatform.Windows))
        {
            _process.Start("explorer.exe", string.Format("/select,\"{0}\"", zipPath));
        }
    }

    private async Task RefreshPressed()
    {
        var currentFolder = _fileSystem.Directory.GetCurrentDirectory();
        _homeViewModel.ClearList();
        _homeViewModel.GetLocalItems(_fileSystem.DirectoryInfo.New(currentFolder).Parent.Parent.FullName);
        await _homeViewModel.DownloadImages();
    }
    private void DoorstopModeSwitch()
    {
        var newValue = !DoorstopEnabled;
        DoorstopEnabled = newValue;
        _settings.SetDoorstopE(newValue);
        UpdateUI();
    }

    private void UseSteamVRPressed()
    {
        var newValue = !UseSteamVR;
        DisableAllVR();
        UseSteamVR = newValue;
        _settings.SetVRMode(Settings.VRMode.SteamVR);
        UpdateUI();
    }

    private void UseOculusPressed()
    {
        var newValue = !UseOculus;
        DisableAllVR();
        UseOculus = newValue;
        _settings.SetVRMode(Settings.VRMode.Oculus);
        UpdateUI();
    }

    private void UseOpenXRPressed()
    {
        var newValue = !UseOpenXR;
        DisableAllVR();
        UseOpenXR = newValue;
        _settings.SetVRMode(Settings.VRMode.OpenXR);
        UpdateUI();
    }

    private void DisableAllVR()
    {
        UseSteamVR = false;
        UseOculus = false;
        UseOpenXR = false;
    }

    private void UpdateUI()
    {
        this.RaisePropertyChanged(nameof(UseSteamVR));
        this.RaisePropertyChanged(nameof(UseOculus));
        this.RaisePropertyChanged(nameof(UseOpenXR));
        this.RaisePropertyChanged(nameof(DoorstopEnabled));
    }

    public ViewModelBase GetViewModel() => this;
}