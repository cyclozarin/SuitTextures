using UnityEngine;
using System.Collections;
using System.Linq;
using System.Threading;

namespace SuitTextures.Hooks
{
    internal class PlayerHook
    {
        internal static void Init()
        {
            On.Player.Start += MMHook_Postfix_ApplySuitTextureOnStart;
        }

        private static IEnumerator MMHook_Postfix_ApplySuitTextureOnStart(On.Player.orig_Start orig, Player self)
        {
            var _orig = orig(self);
            while (_orig.MoveNext())
                yield return _orig.Current;

            string _currentSuitTextureString = PlayerPrefs.GetString("SuitTexture");
            if (self.IsLocal)
            {
                bool _defaultSkin = _currentSuitTextureString == "Default" || _currentSuitTextureString == null;
                if (_defaultSkin)
                    Plugin.ApplySuitTexture(Player.localPlayer, "Default", true);
                else
                    Plugin.ApplySuitTexture(Player.localPlayer, _currentSuitTextureString);
                Plugin.ApplySuitTextureInProperties(self.photonView.Owner, _currentSuitTextureString);
                Plugin.CurrentSuitTextureIndex = _defaultSkin ? -1 : Mathf.Clamp(Plugin.Skins.IndexOf(Plugin.Skins.First((t) => t.name == _currentSuitTextureString)), 0, Plugin.Skins.Count - 1);
                if (PhotonGameLobbyHandler.IsSurface)
                    Plugin.ChangeSkinTextureText();
            }
            else
                yield break;
        }
    }
}
