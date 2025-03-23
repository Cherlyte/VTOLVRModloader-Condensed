using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using SteamQueries.Models;
using UnityEngine;

namespace ModLoader.Assets.ReadyRoom;

public class SettingsList : MonoBehaviour
{
    public const string PrefabName = "Settings List";
    private GameObject _instance;
    private GameObject _mainScreen;
    private GameObject _modsList;

    private Transform _selectionHighlight;

    private async UniTask Start()
    {
        _mainScreen = GameObject.Find("/InteractableCanvas/Canvas/MainScreen") ??
                      throw new Exception("Couldn't find the main screen to disable when viewing the mods page");

        await CreateSettingsPage();
        Hide();
    }

    private async UniTask CreateSettingsPage()
    {
        _instance = AssetBundleLoader.Instance.SpawnPrefab(PrefabName);

        var canvas = GameObject.Find("/InteractableCanvas/Canvas");

        var transform = _instance.transform;
        transform.parent = canvas.transform;
        transform.localPosition = new Vector3(-39.0000267f, 1.81779504f, -2.6265252e-05f);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(1.81778347f, 1.81778336f, 1.81778336f);

        var backButton = transform.Find("BackButton") ??
                         throw new Exception($"Can't find the back button on the '{PrefabName}' prefab");

        var backInteractable = backButton.GetComponent<VRInteractable>();
        backInteractable.OnInteract.AddListener(BackButtonPressed);

        _selectionHighlight = _instance.transform.Find("Mod Scroll View/Viewport/Content/selectionTf");
        if (_selectionHighlight == null)
        {
            Debug.LogError("Can't find the selection highlight for scroll view");
        }
    }

    private void BackButtonPressed()
    {
        _modsList ??= GameObject.Find($"/InteractableCanvas/Canvas/{ModsPage.PrefabName}(Clone)") ??
                       throw new Exception($"Couldn't find the mods list on the canvas");

        Hide();
    }

    private void Hide()
    {
        _instance.SetActive(false);
        _modsList?.SetActive(true);
    }

    public async UniTask Show()
    {
        await PopulateSideBar();
        _instance.SetActive(true);
    }

    private async UniTask PopulateSideBar()
    {
        var scrollViewContent = _instance.transform.Find("Mod Scroll View/Viewport/Content");
        if (scrollViewContent == null)
        {
            Debug.LogError("Couldn't find the content for the scroll view in the mods list");
            return;
        }

        var rectTransform = scrollViewContent.GetComponent<RectTransform>();

        var loadedItems = ModLoader.Instance.LoadedItems.ToArray();

        for (int index = 0; index < loadedItems.Length; index++)
        {
            var steamItem = loadedItems[index].Item;
            var newObject = AssetBundleLoader.Instance.SpawnPrefab("Mod Item Template");

            newObject.transform.SetParent(scrollViewContent, false);
            var objectHeight = ((RectTransform)newObject.transform).rect.height;
            newObject.transform.localPosition = new Vector3(0, -index * objectHeight, 0);

            var script = newObject.GetComponent<VRUIListItemTemplate>();
            script.Setup(steamItem.Title, index, _ =>
            {
                SelectItem(steamItem, newObject.transform);
            });

            if (index == 0)
            {
                // Selecting the first item on the list by default
                SelectItem(steamItem, newObject.transform);
            }

            var selectButton = newObject.transform.Find("selectButton");
            if (selectButton == null)
            {
                Debug.LogError("Couldn't find select button on mod item template");
            }

            // VRIntUIMask in Update() does some logic. This stops errors every frame
            var intUIMask = selectButton.GetComponent<VRIntUIMask>();
            intUIMask.mask = (RectTransform)scrollViewContent.parent;


            await UniTask.WaitForEndOfFrame(this);
        }

        var itemCount = scrollViewContent.childCount - 3;
        // This 50 is the height of the items rounded up
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50 * itemCount);
    }

    private void SelectItem(SteamItem item, Transform scrollViewTransform)
    {
        Debug.Log($"User selected settings for '{item.Title}'");
        _selectionHighlight.position = scrollViewTransform.position;
    }
}