using UCustomPrefabsAPI;

namespace SuitTextures.Hooks
{
    internal class UCPHook
    {
        internal static void Init()
        {
            On.UCustomPrefabsAPI.UCustomPrefabHandler.Initialize += MMHook_Postfix_ApplyTextureToSuit;
        }

        private static void MMHook_Postfix_ApplyTextureToSuit(On.UCustomPrefabsAPI.UCustomPrefabHandler.orig_Initialize orig, UCustomPrefabHandler self)
        {
            orig(self);

            Plugin.ApplyCurrentSuitTexture();
        }
    }
}
