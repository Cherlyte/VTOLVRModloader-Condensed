using System.Threading.Tasks;
using Mod_Manager.Abstractions.VIewModel;
using Mod_Manager.Models;

namespace Mod_Manager.Abstractions;

public interface IPopUpView: IViewModel
{
    Task<PopUpButton> ShowPopUp(string title, string message, string? leftButtonText = null, string? rightButtonText = null);
}