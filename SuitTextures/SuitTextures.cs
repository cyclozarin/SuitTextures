global using Plugin = SuitTextures.SuitTextures;
using BepInEx;
using BepInEx.Logging;
using SuitTextures.Hooks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace SuitTextures
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION), ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, true)]
    public class SuitTextures : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;

        internal new static ManualLogSource Logger { get; private set; } = null!;

        internal static AssetBundle Bundle { get; private set; } = null!;

        internal static List<Texture2D> Skins = [];

        internal static Dictionary<string, Vector2> SkinsScales = new()
        {
            { "cow", new(-5, 3) },
            { "eu", new(-5, -5) },
            { "russia", new(0, -3) },
            { "usa", new(0, -3.5f) },
            { "melon", new(-3, -6.3f) },
            { "zebra",  new(0, -10) },
            { "tiger",  new(-3, -6.3f) },
            { "gold",  new(-3, -6.3f) },
            { "silver",  new(-3, -6.3f) },
            { "spiderman",  new(-10, -10) },
            { "hearts",  new(-5, -5) },
            { "gingerbread",  new(-5, -5) },
            { "wood", new(-2, -5) },
            { "metal", new(-5, -5) },
            { "clown", new(-7, -7) },
            { "target", new(-5, -5) },
            { "sack", new(-5, -5) },
            { "bliss", new(-5, -5) },
            { "water", new(-10, -10) },
            { "fire", new(-10, -10) },
            { "earth", new(-10, -10) },
            { "camouflage", new(-10, -10) },
            { "weed", new(-5, -5) },
            { "catpaws", new(-5, -10) },
            { "bubbles", new(-5, -10) },
            { "pinkguy", new(0, 0) }
        };

        internal static int CurrentSuitTextureIndex = -1;

        private static TextMeshProUGUI _currentSkinText = null!;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            HookAll();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} by {MyPluginInfo.PLUGIN_GUID.Split(".")[0]} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void Start()
        {
            Bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("SuitTextures.Bundles.suittexturesbundle"));

            foreach (var _texture in Bundle.LoadAllAssets<Texture2D>())
            {
                Skins.Add(_texture);
                Logger.LogInfo($"Loaded {_texture.name} suit texture");
            }

            Logger.LogWarning($"Loaded {Skins.Count} suit textures");
        }

        internal static void HookAll()
        {
            Logger.LogDebug("Hooking...");

            PlayerHook.Init();
            FaceCustomizerHook.Init();

            // stolen from morecolors to prevent it from coloring textured suit
            On.PlayerVisor.Update += (orig, self) =>
            {
                orig(self);
                if (CurrentSuitTextureIndex != -1)
                {
                    SkinnedMeshRenderer[] componentsInChildren = self.transform.GetChild(1).GetComponentsInChildren<SkinnedMeshRenderer>();
                    for (int i = 0; i < componentsInChildren.Length; i++)
                    {
                        componentsInChildren[i].material.color = Color.white;
                    }
                }
            };

            Logger.LogDebug("Finished Hooking!");
        }

        internal static void ChangeMaterialTexture(Material material, Texture2D texture)
        {
            material.shader = Shader.Find("NiceShader_Extra");

            material.color = Color.white;

            material.SetTexture("_ExtraTexture", texture);
            material.SetTextureScale("_ExtraTexture", SkinsScales[texture.name]);
        }

        internal static void ResetMaterialTextureAndChangeColor(Material material, Color color)
        {
            material.shader = Shader.Find("NiceShader");

            material.color = color;
        }

        internal static void ApplySuitTexture(string texture, bool reset = false)
        {
            Transform _playerTransform = Player.localPlayer.gameObject.transform.GetChild(1);

            var _headRenderer = _playerTransform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
            var _bodyRenderer = _playerTransform.GetChild(3).gameObject.GetComponent<SkinnedMeshRenderer>();

            if (reset)
            {
                Color _firstColor = new Color(0.1887f, 0.1887f, 0.1887f) with { a = 0.913f };
                Color _secondColor = new Color(0.320f, 0.307f, 0.289f) with { a = 0.749f };

                ResetMaterialTextureAndChangeColor(_headRenderer.materials[0], _firstColor);
                ResetMaterialTextureAndChangeColor(_headRenderer.materials[1], _secondColor);

                ResetMaterialTextureAndChangeColor(_bodyRenderer.materials[0], _firstColor);
                ResetMaterialTextureAndChangeColor(_bodyRenderer.materials[1], _secondColor);

                return;
            }

            Texture2D _currentTexture = Skins.First((t) => t.name == texture);

            ChangeMaterialTexture(_headRenderer.materials[0], _currentTexture);
            ChangeMaterialTexture(_headRenderer.materials[1], _currentTexture);

            ChangeMaterialTexture(_bodyRenderer.materials[0], _currentTexture);
            ChangeMaterialTexture(_bodyRenderer.materials[1], _currentTexture);

            PlayerPrefs.SetString("SuitTexture", texture);
        }

        internal static void NextSkin()
        {
            CurrentSuitTextureIndex++;
            CurrentSuitTextureIndex = Mathf.Clamp(CurrentSuitTextureIndex, -1, Skins.Count - 1);
            ChangeSkinTextureText();
            ApplySuitTexture(Skins[CurrentSuitTextureIndex].name);
        }

        internal static void PreviousSkin()
        {
            CurrentSuitTextureIndex--;
            CurrentSuitTextureIndex = Mathf.Clamp(CurrentSuitTextureIndex, -1, Skins.Count - 1);
            ChangeSkinTextureText();
            if (CurrentSuitTextureIndex == -1)
            {
                ApplySuitTexture(null, true);
                return;
            }
            ApplySuitTexture(Skins[CurrentSuitTextureIndex].name);
        }

        internal static void ResetSkin()
        {
            CurrentSuitTextureIndex = -1;
            ChangeSkinTextureText();
            ApplySuitTexture(null, true);
        }

        internal static void SetSkinTextureTextMesh(TextMeshProUGUI textMesh)
        {
            _currentSkinText = textMesh;
            ChangeSkinTextureText();
        }

        internal static void ChangeSkinTextureText()
        {
            _currentSkinText.text = $"Selected skin texture:\n{(CurrentSuitTextureIndex == -1 ? "Default" : Skins[CurrentSuitTextureIndex].name)}";
        }
    }
}
