global using Plugin = SuitTextures.SuitTextures;
using BepInEx;
using BepInEx.Logging;
using Photon.Pun;
using SuitTextures.Hooks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UCustomPrefabsAPI;
using HarmonyLib;

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

        private static string CurrentSuitTextureName 
        { 
            get
            {
                if (CurrentSuitTextureIndex != 1)
                    return Skins[CurrentSuitTextureIndex].name;
                else
                    return "Default";
            }
        }

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            HookAll();

            PhotonNetwork.AddCallbackTarget(new SuitTextureUpdater());

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

            if (UCPLoaded()) UCPHook.Init();

            // stolen from morecolors to prevent it from coloring textured suit
            On.Player.Update += (orig, self) =>
            {
                orig(self);
                if (self.photonView.Owner.CustomProperties.ContainsKey("Suit texture") && (string)self.photonView.Owner.CustomProperties["Suit texture"] != "Default")
                {
                    Transform _playerTransform = UCPPlayer(self) ? self.transform.GetChild(5) : self.transform.GetChild(1);
                    List<SkinnedMeshRenderer> _meshes = _playerTransform.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();

                    try
                    {
                        if (UCPPlayer(self))
                            _meshes.AddRange(self.transform.GetChild(6).GetComponentsInChildren<SkinnedMeshRenderer>());
                    }
                    catch { }

                    foreach (var _mesh in _meshes)
                        for (int i = 0; i < _mesh.materials.Length; i++)
                            if (!VisorMaterial(_mesh.materials[i]))
                                _mesh.materials[i].color = Color.white;
                }
                if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F))
                {
                    ApplyCurrentSuitTexture();
                    UserInterface.ShowMoneyNotification("Successfully reapplied suit texture", string.Empty, MoneyCellUI.MoneyCellType.MetaCoins);
                }
            };

            Logger.LogDebug("Finished Hooking!");
        }

        internal static void ChangeMaterialTexture(Material material, Texture2D texture)
        {
            if (VisorMaterial(material)) return;

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

        internal static void ApplySuitTexture(Player player, string texture, bool reset = false)
        {
            Transform _playerTransform = UCPPlayer(player) ? player.transform.GetChild(5) : player.transform.GetChild(1);

            List<SkinnedMeshRenderer> _meshes = _playerTransform.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();

            if (UCPPlayer(player))
                _meshes.AddRange(player.transform.GetChild(6).GetComponentsInChildren<SkinnedMeshRenderer>());

            if (player == Player.localPlayer)
            {
                ApplySuitTextureInProperties(player.photonView.Owner, texture);
                PlayerPrefs.SetString("SuitTexture", texture);
            }

            if (reset && !UCPPlayer(player))
            {
                Color _firstColor = new Color(0.1887f, 0.1887f, 0.1887f) with { a = 0.913f };
                Color _secondColor = new Color(0.320f, 0.307f, 0.289f) with { a = 0.749f };

                foreach (var _mesh in _meshes)
                    for (int i = 0; i < _mesh.materials.Length; i++)
                        if (!VisorMaterial(_mesh.materials[i]))
                        {
                            if (_mesh.materials[i].name.Contains("M_Player"))
                                ResetMaterialTextureAndChangeColor(_mesh.materials[i], _firstColor);
                            else if (_mesh.materials[i].name.Contains("M_Player 1"))
                                ResetMaterialTextureAndChangeColor(_mesh.materials[i], _secondColor);
                        }

                return;
            }

            Texture2D _currentTexture = Skins.First((t) => t.name == texture);

            foreach (var _mesh in _meshes)
                for (int i = 0; i < _mesh.materials.Length; i++)
                    ChangeMaterialTexture(_mesh.materials[i], _currentTexture);

        }

        internal static void ApplyCurrentSuitTexture()
        {
            ApplySuitTexture(Player.localPlayer, CurrentSuitTextureName, CurrentSuitTextureIndex == -1);
        }

        internal static void ApplySuitTextureInProperties(Photon.Realtime.Player player, string texture)
        {
            Hashtable _playerProperties = player.CustomProperties;
            if (_playerProperties.ContainsKey("Suit texture"))
                _playerProperties["Suit texture"] = texture;
            else
                _playerProperties.Add("Suit texture", texture ?? "Default");
            player.SetCustomProperties(_playerProperties);
        }

        internal static bool UCPLoaded()
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("UCustomPrefabsAPI.ContentWarning");
        }

        internal static bool UCPPlayer(Player player)
        {
            if (!UCPLoaded()) return false;

            return player.GetComponentInChildren<PrefabTemplate>();
        }

        internal static bool VisorMaterial(Material material)
        {
            return material.name.Contains("M_PlayerVisor");
        }

        internal static void NextSkin()
        {
            CurrentSuitTextureIndex++;
            CurrentSuitTextureIndex = Mathf.Clamp(CurrentSuitTextureIndex, -1, Skins.Count - 1);
            ChangeSkinTextureText();
            FaceCustomizerHook.PlayChooseSound();
            ApplyCurrentSuitTexture();
        }

        internal static void PreviousSkin()
        {
            CurrentSuitTextureIndex--;
            CurrentSuitTextureIndex = Mathf.Clamp(CurrentSuitTextureIndex, -1, Skins.Count - 1);
            ChangeSkinTextureText();
            FaceCustomizerHook.PlayChooseSound();
            if (CurrentSuitTextureIndex == -1)
            {
                ApplySuitTexture(Player.localPlayer, "Default", true);
                return;
            }
            ApplyCurrentSuitTexture();
        }

        internal static void ResetSkin()
        {
            CurrentSuitTextureIndex = -1;
            ChangeSkinTextureText();
            FaceCustomizerHook.PlayResetSound();
            ApplyCurrentSuitTexture();
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