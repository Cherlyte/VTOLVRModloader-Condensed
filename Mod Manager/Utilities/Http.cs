using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Mod_Manager.Abstractions;
using Serilog;

namespace Mod_Manager.Utilities;

internal class Http : IHttp
{
    private readonly IProcess _process;
    private readonly IRuntimeInfo _runtimeInfo;

    private readonly HttpClient _client = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", $"VTOL VR Mod Loader/{Assembly.GetExecutingAssembly().GetName().Version}" }
        }
    };

    public Http(IProcess process, IRuntimeInfo runtimeInfo)
    {
        _process = process;
        _runtimeInfo = runtimeInfo;
    }

    public async Task<Bitmap?> GetImageAsync(string url)
    {
        try
        {
            var stream = await GetImageStreamAsync(url);
            
            // This solves the lag when scrolling with many images.
            var bitmap = Bitmap.DecodeToWidth(stream,100);
            return bitmap;
        }
        catch (Exception e)
        {
            Log.Error("Failed to download image from '{URL}'. {Exception}", url, e.Message);
            return null;
        }
    }

    public async Task<Stream?> GetImageStreamAsync(string url)
    {
        try
        {
            var bytes = await _client.GetByteArrayAsync(url);
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
        catch (Exception e)
        {
            Log.Error("Failed to download image from '{URL}'. {Exception}", url, e.Message);
            return null;
        }
    }

    public void OpenUrlFromSteam(string url)
    {
        var fullUrl = $"https://steamcommunity.com/linkfilter/?url={url}";
        OpenUrl(url);
    }

    public void OpenInSteam(string url)
    {
        var escapedString = url.Replace("&", "^&");
        _process.Start(new ProcessStartInfo("cmd", $"/c start steam://openurl/{escapedString}") { CreateNoWindow = true });
    }

    public void OpenUrl(string url)
    {
        try
        {
            _process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (_runtimeInfo.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                _process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (_runtimeInfo.IsOSPlatform(OSPlatform.Linux))
            {
                _process.Start("xdg-open", url);
            }
            else
            {
                throw;
            }
        }
    }
}