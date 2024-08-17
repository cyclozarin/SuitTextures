using UnityEngine;
using System.Collections;
using System.Linq;

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
            if (_currentSuitTextureString != null)
            {
                Plugin.ApplySuitTexture(_currentSuitTextureString);
                Plugin.CurrentSuitTextureIndex = Mathf.Clamp(Plugin.Skins.IndexOf(Plugin.Skins.First((t) => t.name == _currentSuitTextureString)), 1, Plugin.Skins.Count - 1);
                if (PhotonGameLobbyHandler.IsSurface)
                    Plugin.ChangeSkinTextureText();
            }
            else
                yield break;
        }
    }
}
