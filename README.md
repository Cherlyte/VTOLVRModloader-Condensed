# VTOLVR Modloader Condensed
The VTOL VR Modloader, Condensed. As in the act of condensing Steam back into Water, thus removing it from the loop.

This is a fork of the [VTOL VR Modloader](https://gitlab.com/vtolvr-mods/ModLoader) without any Steam implementations present, thus reducing bloat and killing the annoying SteamQueries application that runs in the background at all times.<br />
It also includes a few QoL things i found nice, such as the Play button in the manager starting VTOLVR through its executable, and a new "Enable by Default" option that loads the modloader with the game without the need for special launch arguments or the manager.

## Creating Mods
You can find more information about how to create mods for the ModLoader in here.

## Building your own ModLoader
The ModLoader incorporates a very janky use of .NET 6.0, 7.0, 8.0 and Framework 4.6/Standard 2.0, they are required for building the ModLoader<br>
It's recommended you use a IDE such as Visual Studio 2022 to build the ModLoader, however, there is a powershell script provided that should do it for you.<br>
`.\build_v6_modloader.ps1 -ModLoaderFolder "A:\Path\To\ModLoader" -VTOLVRFolder "A:\Path\To\VTOL VR"`

## Credits
Of course, good things are hardly made by only one person, which is why we have to remind ourselves of the people who worked on it.<br>

[The Original Modloader Team @ GitLab](https://gitlab.com/vtolvr-mods/ModLoader#contributors)<br>
Cherlyte - For "Condensing" the ModLoader
