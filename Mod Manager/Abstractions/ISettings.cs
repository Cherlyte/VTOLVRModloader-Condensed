using Mod_Manager.Models;

namespace Mod_Manager.Abstractions;

internal interface ISettings
{
    public Settings.VRMode GetVRMode();
    public bool GetDoorstopE();
    public void SetDoorstopE(bool value);
    public void SetVRMode(Settings.VRMode newValue);
}