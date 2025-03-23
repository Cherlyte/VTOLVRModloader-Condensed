using System.Threading.Tasks;

namespace Mod_Manager.Abstractions.VIewModel;

internal interface IHomeViewModel : IViewModel
{
    void ClearList();
    void GetLocalItems(string baseGameFolder);
    Task DownloadImages();
}