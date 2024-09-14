using UnityEngine;
using System.Linq;

namespace SuitTextures.Hooks
{
    internal class PlayerHook
    {
        internal static void Init()
        {
            On.Player.Awake += MMHook_Postfix_ApplySuitTextureOnStart;
        }

        private static void MMHook_Postfix_ApplySuitTextureOnStart(On.Player.orig_Awake orig, Player self)
        {
            orig(self);

            if (self.IsLocal)
            {
                string _currentSuitTextureString = PlayerPrefs.GetString("SuitTexture");
                bool _defaultSkin = _currentSuitTextureString == "Default" || _currentSuitTextureString == null;

                Plugin.CurrentSuitTextureIndex = _defaultSkin ? -1 : Mathf.Clamp(Plugin.Skins.IndexOf(Plugin.Skins.First((t) => t.name == _currentSuitTextureString)), 0, Plugin.Skins.Count - 1);

                Plugin.ApplySuitTextureInProperties(self.photonView.Owner, _currentSuitTextureString);
                Plugin.ApplyCurrentSuitTexture();

                if (PhotonGameLobbyHandler.IsSurface)
                    Plugin.ChangeSkinTextureText();
            }
        }
    }
}
