using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace SuitTextures.Hooks
{
    internal class FaceCustomizerHook
    {
        private static PlayerCustomizer _playerCustomizer = null!;

        internal static void Init()
        {
            On.SurfaceNetworkHandler.InitSurface += MMHook_Postfix_SpawnSkinChangeButtons;
        }

        private static void MMHook_Postfix_SpawnSkinChangeButtons(On.SurfaceNetworkHandler.orig_InitSurface orig, SurfaceNetworkHandler self)
        {
            orig(self);

            Transform _canvasTransform = GameObject.Find("Tools/PlayerCustomizerMachine/WhyDoesThisObjectExist/WorldSpace").transform;
            _playerCustomizer = GameObject.Find("Tools/PlayerCustomizerMachine").GetComponent<PlayerCustomizer>();
            GameObject _pasteButton = _playerCustomizer.pastButton.transform.parent.gameObject;

            _pasteButton.GetComponent<RectTransform>().localPosition = _pasteButton.GetComponent<RectTransform>().localPosition with { y = -190 };

            GameObject _hatSelect = _canvasTransform.Find("Hats").gameObject;
            GameObject _skinTextureSelect = Object.Instantiate(_hatSelect, _hatSelect.transform.position, new Quaternion(0, 0, 0, 0), _canvasTransform);
            _skinTextureSelect.name = "Skin texture select";

            _skinTextureSelect.GetComponent<RectTransform>().localPosition = _skinTextureSelect.GetComponent<RectTransform>().localPosition with { x = 643 };

            Button _previousSkinButton = _skinTextureSelect.transform.Find("Left").GetComponentInChildren<Button>();
            Button _nextSkinButton = _skinTextureSelect.transform.Find("Right").GetComponentInChildren<Button>();
            Button _resetSkinButton = _skinTextureSelect.transform.Find("ClearHat").GetComponent<Button>();
            TextMeshProUGUI _skinTextureName = _skinTextureSelect.transform.Find("BG/HatName").GetComponentInChildren<TextMeshProUGUI>();

            Object.Destroy(_skinTextureName.GetComponent<UnityEngine.Localization.PropertyVariants.GameObjectLocalizer>());
            _skinTextureName.autoSizeTextContainer = true;
            _skinTextureName.fontSize = 20;

            _previousSkinButton.onClick.RemoveAllListeners();
            _nextSkinButton.onClick.RemoveAllListeners();
            _resetSkinButton.onClick.RemoveAllListeners();

            _previousSkinButton.onClick.AddListener(Plugin.PreviousSkin);
            _nextSkinButton.onClick.AddListener(Plugin.NextSkin);
            _resetSkinButton.onClick.AddListener(Plugin.ResetSkin);

            TextMeshProUGUI _resetSkinButtonText = _resetSkinButton.GetComponentInChildren<TextMeshProUGUI>();
            Object.Destroy(_resetSkinButtonText.GetComponent<UnityEngine.Localization.PropertyVariants.GameObjectLocalizer>());
            _resetSkinButtonText.text = "RESET SKIN";

            Plugin.SetSkinTextureTextMesh(_skinTextureName);
        }

        internal static void PlayChooseSound()
        {
            _playerCustomizer.clickSound.Play(_playerCustomizer.transform.position);
        }

        internal static void PlayResetSound()
        {
            _playerCustomizer.backSound.Play(_playerCustomizer.transform.position);
        }
    }
}
