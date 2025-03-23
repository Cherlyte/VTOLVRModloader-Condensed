using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Mod_Manager.Abstractions;
using Serilog;

namespace Mod_Manager.Models;

internal sealed class LogsZipGenerator: ILogsZipGenerator
{
    private readonly IRuntimeInfo _runtimeInfo;
    private readonly IFileSystem _fileSystem;
    private readonly IFileManager _fileManager;

    public LogsZipGenerator(IRuntimeInfo runtimeInfo, IFileSystem fileSystem, IFileManager fileManager)
    {
        _runtimeInfo = runtimeInfo;
        _fileSystem = fileSystem;
        _fileManager = fileManager;
    }

    public async Task<string> CollectLogs()
    {
        Log.Information("Collecting Logs");
        var fileFriendlyDateTime = DateTime.Now.ToString().Replace('/', '-').Replace(':', '-');
        var zipFilePath = Path.Combine(_fileManager.GetVtolDirectory(), "@Mod Loader", $"Logs {fileFriendlyDateTime}.zip");
        await using var zipStream = _fileSystem.File.Create(zipFilePath);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
        
        var playerLogPath = GetPlayerLogPath();
        if (!string.IsNullOrEmpty(playerLogPath))
        {
            try
            {
                await CreateEntryFromFile(archive, playerLogPath, "Player.log");
            }
            catch (Exception e)
            {
                Log.Warning(e, "Unable to copy game log into zip");
            }   
        }

        var doorstopFilePath = GetDoorstopFile();
        if (!string.IsNullOrEmpty(doorstopFilePath))
        {
            await CreateEntryFromFile(archive, doorstopFilePath, "doorstop_config.ini");
        }

        var loadOnStartBytes = GetLoadOnStartBytes().ToArray();
        var wasLoSFound = false;
        if (loadOnStartBytes.Any())
        {
            var entry = archive.CreateEntry($"{LoadOnStartManager.LoadOnStartFile}.json");
            await using var stream = entry.Open();
            await stream.WriteAsync(loadOnStartBytes);
            wasLoSFound = true;
        }
        
        

        var infoEntry = archive.CreateEntry("Info.txt");
        await using (var infoStream = infoEntry.Open())
        {
            await infoStream.WriteAsync(Encoding.Default.GetBytes(PrintGenericInfo()));

            if (!wasLoSFound)
            {
                await infoStream.WriteAsync(Encoding.Default.GetBytes(
                    $"The file {LoadOnStartManager.LoadOnStartFile} was not found on Steam Remote Storage"));
            }
        }
        
        // This should be last so that any log messages in the steps above get shown
        var fromDays = TimeSpan.FromDays(3);
        var pastThreeDaysLogs = GetLastXLogs($@"@Mod Loader\Mod Manager\{App.LogsDirectoryName}", fromDays);
        foreach (var logInfo in pastThreeDaysLogs)
        {
            var entry = archive.CreateEntry($"Mod Manager Logs/{logInfo.Name}");
            entry.LastWriteTime = logInfo.LastWriteTime;
            
            await using var fileStream = _fileSystem.File.Open(logInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await using var stream = entry.Open();
            await fileStream.CopyToAsync(stream);
        }
        
        pastThreeDaysLogs = GetLastXLogs(@"@Mod Loader\Mod Uploader\Logs", fromDays);
        foreach (var logInfo in pastThreeDaysLogs)
        {
            var entry = archive.CreateEntry($"Mod Uploader Logs/{logInfo.Name}");
            entry.LastWriteTime = logInfo.LastWriteTime;
            
            await using var fileStream = _fileSystem.File.Open(logInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await using var stream = entry.Open();
            await fileStream.CopyToAsync(stream);
        }
        
        pastThreeDaysLogs = GetLastXLogs(@"@Mod Loader\SteamQueries\Logs", fromDays);
        foreach (var logInfo in pastThreeDaysLogs)
        {
            var entry = archive.CreateEntry($"SteamQueries Logs/{logInfo.Name}");
            entry.LastWriteTime = logInfo.LastWriteTime;
            
            await using var fileStream = _fileSystem.File.Open(logInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            await using var stream = entry.Open();
            await fileStream.CopyToAsync(stream);
        }
        
        return zipFilePath;
    }

    private string GetPlayerLogPath()
    {
        if (_runtimeInfo.IsOSPlatform(OSPlatform.Linux))
        {
            var home = _fileSystem.DirectoryInfo.New(
                _runtimeInfo.GetFolderPath(Environment.SpecialFolder.Personal));

            var folder = Path.Combine(home.FullName, ".config", "unity3d", "Boundless Dynamics, LLC", "VTOLVR");
            if (_fileSystem.Directory.Exists(folder))
            {
                var filePath = Path.Combine(folder, "Player.log");
                if (_fileSystem.File.Exists(filePath))
                {
                    return filePath;
                }
                Log.Information("Couldn't find the player.log file at {0}", filePath);
            }
        }
        else if (_runtimeInfo.IsOSPlatform(OSPlatform.Windows))
        {
            var userprofile = _fileSystem.DirectoryInfo.New(
                _runtimeInfo.GetFolderPath(Environment.SpecialFolder.UserProfile));

            var folder =Path.Combine(userprofile.FullName,"AppData", "LocalLow", "Boundless Dynamics, LLC", "VTOLVR");
            if (_fileSystem.Directory.Exists(folder))
            {
                var filePath = Path.Combine(folder, "Player.log");
                if (_fileSystem.File.Exists(filePath))
                {
                    return filePath;
                }
                Log.Information("Couldn't find the player.log file at {0}", filePath);
            }
        }
        return string.Empty;
    }

    private IEnumerable<IFileSystemInfo> GetLastXLogs(string relativePath, TimeSpan timeBack)
    {
        var path = Path.Combine(_fileManager.GetVtolDirectory(), relativePath);

        if (!_fileSystem.Directory.Exists(path))
        {
            Log.Warning("Could not find log path of {LogPath}", path);
            return Enumerable.Empty<IFileSystemInfo>();
        }

        var files = _fileSystem.DirectoryInfo.New(path).GetFileSystemInfos("*.txt");
        var now = DateTime.Now;
        var recentLogFiles = new List<IFileSystemInfo>();
        
        foreach (var logFile in files)
        {
            if (now.Subtract(logFile.CreationTime) <= timeBack)
            {
                recentLogFiles.Add(logFile);
            }
        }

        return recentLogFiles;
    }

    private string GetDoorstopFile()
    {
        var vtolDir = _fileManager.GetVtolDirectory();
        var doorstopPath = Path.Combine(vtolDir, "doorstop_config.ini");

        if (!_fileSystem.File.Exists(doorstopPath))
        {
            Log.Warning("No doorstop_config.ini was found");
            return string.Empty;
        }

        return doorstopPath;
    }

    private IEnumerable<byte> GetLoadOnStartBytes()
    {
        var vtolPath = _fileManager.GetVtolDirectory();
        var nfilePath = Path.Combine(vtolPath, LoadOnStartManager.LoadOnStartFile);
        if (!File.Exists(nfilePath))
        {
            Log.Warning("The file {FileName} does not exist", LoadOnStartManager.LoadOnStartFile);
            return Enumerable.Empty<byte>();
        }

        return File.ReadAllBytes(nfilePath);
    }

    private string PrintGenericInfo()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
        string version = fvi.FileVersion;
        
        var builder = new StringBuilder();

        builder.AppendLine($"Created Time:{DateTime.Now}");
        builder.AppendLine($"UTC Time:{DateTime.UtcNow}");
        builder.AppendLine($"Mod Manager Version:{version}");
        builder.AppendLine($"Current Directory:{_fileSystem.Directory.GetCurrentDirectory()}");

        return builder.ToString();
    }

    private async Task CreateEntryFromFile(ZipArchive archive, string sourcePath, string entryName)
    {
        var entry = archive.CreateEntry(entryName);
        await using var gameLogStream = _fileSystem.File.OpenRead(sourcePath);
        await using var zipEntryStream = entry.Open();
        await gameLogStream.CopyToAsync(zipEntryStream);
    }
}