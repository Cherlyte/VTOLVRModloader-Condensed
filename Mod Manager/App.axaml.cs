using System;
using System.IO.Abstractions;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod_Manager.Abstractions;
using Mod_Manager.Abstractions.ConfigParser;
using Mod_Manager.Abstractions.VIewModel;
using Mod_Manager.Models;
using Mod_Manager.Utilities;
using Mod_Manager.ViewModels;
using Mod_Manager.Views;
using Mod_Manager.Wrappers.ConfigParser;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using Serilog.Formatting.Compact;
using SteamQueries.Models;

namespace Mod_Manager;

public partial class App : Application
{
    public static uint UploaderAppId = 3018440;
    public static uint LoaderAppId = 3018410;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(e =>
        {
            Log.Error(e, "An exception has thrown");
        });

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Log.Error("UnhandledException {Exception}", args.ExceptionObject);
        };
        
        var builder = Host.CreateApplicationBuilder();
            
        // Models
        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<ILoadOnStartManager, LoadOnStartManager>();
        builder.Services.AddSingleton<IRuntimeInfo, RuntimeInfo>();
        builder.Services.AddSingleton<IFileManager, FileManager>();
        builder.Services.AddSingleton<IConfigParser, WrappedConfigParser>();
        builder.Services.AddSingleton<IDoorstopManager, DoorstopManager>();
        builder.Services.AddSingleton<IInit, Init>();
        builder.Services.AddSingleton<IHttp, Http>();
        builder.Services.AddSingleton<ILogsZipGenerator, LogsZipGenerator>();
        builder.Services.AddSingleton<IProcess, Process>();
        builder.Services.AddSingleton<ISettings, Settings>();

        // View Models
        builder.Services.AddSingleton<IHeaderBarViewModel, HeaderBarViewModel>();
        builder.Services.AddSingleton<IHomeViewModel, HomeViewModel>();
        builder.Services.AddSingleton<IMainWindowViewModel, MainWindowViewModel>();
        builder.Services.AddSingleton<IPopUpView, PopUpViewModel>();
        
        using var host = builder.Build();
            
        var services = host.Services;
        var fileSystem = services.GetService<IFileSystem>();
        CreateLogger(fileSystem);
            
        var window = new MainWindow { DataContext = services.GetService<IMainWindowViewModel>() };

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            Log.Error("Application Lift Time is not type of {DeskstopType} it is {RealType}", nameof(IClassicDesktopStyleApplicationLifetime), ApplicationLifetime?.GetType());
            base.OnFrameworkInitializationCompleted();
            return;
        }
        await StartUpLogic(services.GetService<IHomeViewModel>(),
            services.GetService<IInit>(),
            fileSystem,
            desktop,
            window,
            services.GetService<IDoorstopManager>());
    }

    internal async Task StartUpLogic(IHomeViewModel homeViewModel,
        IInit init,
        IFileSystem fileSystem,
        IClassicDesktopStyleApplicationLifetime desktop, 
        MainWindow mainWindow,
        IDoorstopManager doorstopManager)
    {
        if (!init.IsInGameFiles())
        {
            var messageBoxParams = new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Can't find VTOL VR",
                ContentMessage = "Please install the Mod Loader in the same drive as the game VTOL VR"
            };
            await MessageBoxManager.GetMessageBoxStandard(messageBoxParams).ShowAsync();
            desktop.Shutdown(2);
            return;
        }

        if (init.HasOldModLoaderInstalled())
        {
            var messageBoxParams = new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Old Mod Loader Detected!",
                ContentMessage = "Please uninstall the old mod loader and verify game files first.\nThis can be done by heading to Settings > Uninstall in the old mod loader or manually deleting the VTOLVR_ModLoader folder"
            };

            await MessageBoxManager.GetMessageBoxStandard(messageBoxParams).ShowAsync();
            desktop.Shutdown(3);
            return;
        }
        doorstopManager.CheckForOldFile();
            
        desktop.MainWindow = mainWindow;
        var currentFolder = fileSystem.Directory.GetCurrentDirectory();
        var vtolvrFolder = fileSystem.DirectoryInfo.New(currentFolder).Parent.Parent;
        
        homeViewModel.GetLocalItems(vtolvrFolder.FullName);
        await homeViewModel.DownloadImages();
        base.OnFrameworkInitializationCompleted();
    }

    private static void CreateLogger(IFileSystem fileSystem)
    {
        var currentTime = DateTime.Now;
        var logPath = LogsDirectoryName + $"/Log_{currentTime.Ticks}.txt";
            
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File(new CompactJsonFormatter(), logPath, shared:true)
            .CreateLogger();

        var currentDir = fileSystem.Directory.GetCurrentDirectory();
        Log.Information("Program Started in '{CURRENTFOLDER}'", currentDir);
    }

    public const string LogsDirectoryName = "Logs";
}