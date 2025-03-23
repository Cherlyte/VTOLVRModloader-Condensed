param (
    [Parameter(Mandatory = $true )]
    [string]$ModLoaderFolder,

    [Parameter(Mandatory = $true )]
    [string]$VTOLVRFolder
)

Write-Output "Publishing Mod Loader"
dotnet publish "$ModLoaderFolder\ModLoader\Mod Loader.csproj" --nologo --configuration Release -consoleLoggerParameters:ErrorsOnly

Write-Output "Deleting Managed Folder"
Get-ChildItem -Path "$VTOLVRFolder\@Mod Loader\Managed" -Include *.* -File -Recurse | foreach {
    $_.Delete()
}

Write-Output "Publishing Mod Manager"
dotnet publish "$ModLoaderFolder\Mod Manager\Mod Manager.csproj"-r win10-x64 -p:PublishSingleFile=true
Write-Output "Deleting Mod Manager folder and writing published version."
Get-ChildItem -Path "$VTOLVRFolder\@Mod Loader\Mod Manager" -Include *.* -File -Recurse | foreach {
    $_.Delete()
}
$modManager = Get-ChildItem -Path "$ModLoaderFolder\Mod Manager\bin\Debug\net6.0\win10-x64\publish" -Name
foreach ($item in $modManager)
{
    Copy-Item "$ModLoaderFolder\Mod Manager\bin\Debug\net6.0\win10-x64\publish\$item" -Destination "$VTOLVRFolder\@Mod Loader\Mod Manager\$item"
}
Write-Output "Yup yup!"

Write-Output "Moving new files into Managed folder"

$gameItems = Get-ChildItem -Path "$VTOLVRFolder\VTOLVR_Data\Managed" -Name
$myItems = Get-ChildItem -Path "$ModLoaderFolder\ModLoader\bin\Release\netstandard2.0\publish" -Name
$itemsToMove = New-Object System.Collections.Generic.List[System.String]

foreach ($item in $myItems)
{
    if ($gameItems -notcontains $item)
    {
        $itemsToMove.Add($item)
    }
}

foreach ($item in $itemsToMove)
{
    Copy-Item "$ModLoaderFolder\ModLoader\bin\Release\netstandard2.0\publish\$item" -Destination "$VTOLVRFolder\@Mod Loader\Managed\$item"
}

Write-Output "Moving Asset Bundle"

$assetsGamePath = "$VTOLVRFolder\@Mod Loader\assets"
$assetsUnityPath = "$ModLoaderFolder\Asset Bundle Project\Assets\@Mod Loader\Output\assets"

$assetsGamePathExists = Test-Path $assetsGamePath
$assetsUnityPathExists = Test-Path $assetsUnityPath
if ($assetsUnityPathExists)
{
    if ($assetsGamePathExists)
    {
        Remove-Item $assetsGamePath
    }
    
    Copy-Item $assetsUnityPath -Destination $assetsGamePath
    Write-Output "Copied Asset Bundle from Unity to game files"
}

$assetsGamePathExists = Test-Path $assetsGamePath
if (-not $assetsGamePathExists)
{
    Write-Error "The Asset bundle doesn't exist!!!"
}
