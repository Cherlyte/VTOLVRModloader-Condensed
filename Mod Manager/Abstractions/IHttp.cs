using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace Mod_Manager.Abstractions;

internal interface IHttp
{
    Task<Bitmap?> GetImageAsync(string url);
    Task<Stream?> GetImageStreamAsync(string url);
    void OpenUrlFromSteam(string url);
    void OpenUrl(string url);
    void OpenInSteam(string url);
}