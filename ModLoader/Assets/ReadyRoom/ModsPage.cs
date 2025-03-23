using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using SteamQueries.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ModLoader.Assets.ReadyRoom
{
    internal class ModsPage : MonoBehaviour
    {
        public const string PrefabName = "Mods List";
        private GameObject _instance;
        private GameObject _mainScreen;
        private Transform _selectionHighlight;
        private RawImage _selectedRawImage;
        private Text _selectedTitleText;
        private Text _selectedDescriptionText;
        private Text _loadButtonText;
        private VRInteractable _loadInteractable;
        private SettingsList _settingsList;

        private async UniTaskVoid Start()
        {
            _mainScreen = GameObject.Find("/InteractableCanvas/Canvas/MainScreen") ??
                          throw new Exception("Couldn't find the main screen to disable when viewing the mods page");

            CreateModsButton();
            await CreateModsPage();
            ReStartCanvas();
        }

        private async UniTask CreateModsPage()
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

            var settingsButton = transform.Find("Settings Button") ??
                             throw new Exception($"Can't find the settings button on the '{PrefabName}' prefab");

            var settingsInteractable = settingsButton.GetComponent<VRInteractable>();
            settingsInteractable.OnInteract.AddListener(SettingsButtonPressed);

            _selectionHighlight = _instance.transform.Find("Scroll View/Viewport/Content/selectionTf");
            if (_selectionHighlight == null)
            {
                Debug.LogError("Can't find the selection highlight for scroll view");
            }

            FindLoadButton();
            GetSelectedItemHolder();
            await PopulateSideBar();
            Hide();
            DisplayModLoadersBirthday();
        }

        private void CreateModsButton()
        {
            var button = AssetBundleLoader.Instance.SpawnPrefab("ReadyRoom_ModsButton");
            var gamePanel = GameObject.Find("/InteractableCanvas/Canvas/MainScreen/StandardMenu/GamePanel");
            if (gamePanel == null)
            {
                throw new Exception("Couldn't find the Game Panel in the ready room");
            }


            if (_mainScreen == null)
            {
                throw new Exception("Couldn't find the main screen to disable when viewing the mods page");
            }

            button.transform.SetParent(gamePanel.transform, false);
            button.transform.localPosition = new Vector3(-811, -411.700012f, 0);
            button.transform.localRotation = Quaternion.Euler(0, 0, 0);
            button.transform.localScale = new Vector3(1.84723699f, 1.84723783f, 1.84723783f);

            var interactable = button.GetComponent<VRInteractable>();
            interactable.OnInteract.AddListener(Show);
        }

        private void GetSelectedItemHolder()
        {
            var selectedItemHolder = _instance.transform.Find("MainSelectMask/Selected Item Holder");
            if (selectedItemHolder == null)
            {
                Debug.LogError("Can't find the selected item holder");
            }

            _selectedRawImage = selectedItemHolder.Find("Item Image").GetComponent<RawImage>();
            _selectedTitleText = selectedItemHolder.Find("Item Title").GetComponent<Text>();
            _selectedDescriptionText = selectedItemHolder.Find("Item Description").GetComponent<Text>();
        }

        private void BackButtonPressed()
        {
            _mainScreen.SetActive(true);
            Hide();
        }

        private void SettingsButtonPressed()
        {
            _settingsList ??= GameObject.FindObjectOfType<SettingsList>() ??
                              throw new Exception("Couldn't find SettingsList script");

            _instance.SetActive(false);
            _settingsList.Show();
        }

        private void Show()
        {
            _instance.SetActive(true);
            _mainScreen.SetActive(false);
        }

        private void Hide()
        {
            _instance.SetActive(false);
        }

        private void DisplayModLoadersBirthday()
        {
            var mask = _instance.transform.Find("Mod Loader's Birthday Mask");
            if (mask == null)
            {
                Debug.LogError("Couldn't find 'Mod Loader's Birthday Mask'");
                return;
            }

            var time = DateTime.Now;
            mask.gameObject.SetActive(time is { Month: 4, Day: 28 });
        }

        private async UniTask PopulateSideBar()
        {

            var scrollViewContent = _instance.transform.Find("Scroll View/Viewport/Content");
            if (scrollViewContent == null)
            {
                Debug.LogError("Couldn't find the content for the scroll view in the mods list");
                return;
            }

            var rectTransform = scrollViewContent.GetComponent<RectTransform>();
            var localVisibleItems = ModLoader.Instance.FindLocalItems().Where(item => item.MetaData is { ShowOnMainList: true }).ToArray();
            var visibleItems = localVisibleItems.ToArray();

            for (int index = 0; index < visibleItems.Length; index++)
            {
                var steamItem = visibleItems[index];

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
            }

            var itemCount = scrollViewContent.childCount - 3;
            // This 50 is the height of the items rounded up
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50 * itemCount);
        }

        private void SelectItem(SteamItem item, Transform scrollViewTransform)
        {
            Debug.Log($"User selected '{item.Title}'");
            _selectionHighlight.position = scrollViewTransform.position;
            _selectedTitleText.text = item.Title;
            _selectedDescriptionText.text = item.Description;
            if (string.IsNullOrEmpty(item.PreviewImageUrl))
            {
                _selectedRawImage.texture = new Texture2D(1, 1);
            }
            else
            {
                StartCoroutine(DownloadImage(item.PreviewImageUrl));
            }



            var isLoaded = ModLoader.Instance.IsItemLoaded(item.Directory);
            _loadButtonText.text = isLoaded ? "Disable" : "Load";

            _loadInteractable.interactableName = (isLoaded ? "Disable" : "Load") + $" {item.Title}";
            _loadInteractable.OnInteract.RemoveAllListeners();
            _loadInteractable.OnInteract.AddListener(async void () =>
            {
                if (isLoaded)
                {
                    await ModLoader.Instance.DisableSteamItem(item);
                    isLoaded = false;
                }
                else
                {
                    isLoaded = await ModLoader.Instance.LoadSteamItem(item);
                }

                _loadButtonText.text = isLoaded ? "Disable" : "Load";
                _loadInteractable.interactableName = (isLoaded ? "Disable" : "Load") + $" {item.Title}";
            });
        }

        private IEnumerator DownloadImage(string url)
        {
            using var webRequest = UnityWebRequestTexture.GetTexture(url);

            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download Steam Image " + webRequest.error);
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(webRequest);
                _selectedRawImage.texture = texture;
            }
        }

        private void FindLoadButton()
        {
            var loadButton = _instance.transform.Find("Load Button") ??
                throw new Exception($"Can't find the load button on the '{PrefabName}' prefab");

            _loadInteractable = loadButton.GetComponent<VRInteractable>() ??
                                throw new Exception($"Can't find the type {nameof(VRInteractable)} on the load button");

            _loadButtonText = loadButton.GetComponentInChildren<Text>() ??
                              throw new Exception($"Can't find the type {nameof(Text)} on the children of load button");
        }

        private void ReStartCanvas()
        {
            var canvas = GameObject.Find("/InteractableCanvas");

            if (canvas == null)
            {
                Debug.LogError("Couldn't find the object 'InteractableCanvas'");
                return;
            }

            var script = canvas.GetComponent<VRPointInteractableCanvas>();

            if (script == null)
            {
                Debug.LogError($"Couldn't find the script {nameof(VRPointInteractableCanvas)} on the object '{canvas.name}'");
                return;
            }

            Debug.Log("Called Start");
            script.Start();
        }
    }
}