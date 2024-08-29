using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;

namespace SuitTextures
{
    internal class SuitTextureUpdater : IInRoomCallbacks
    {
        public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            if (!targetPlayer.IsLocal)
            {
                string _changedSuitTexture = null!;
                if (changedProps.ContainsKey("Suit texture"))
                {
                    _changedSuitTexture = (string)changedProps["Suit texture"];
                }
                Player _cwPlayer = PlayerHandler.instance.players.First((player) => player.photonView.Owner == targetPlayer);
                if (_changedSuitTexture != "Default")
                    Plugin.ApplySuitTexture(_cwPlayer, _changedSuitTexture);
                else
                    Plugin.ApplySuitTexture(_cwPlayer, _changedSuitTexture, true);
                Plugin.Logger.LogDebug($"targetPlayer: {targetPlayer.NickName} changedProps: {changedProps}");
            }
        }

        public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) { }
        public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {}
        public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {}
        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {}
    }
}
