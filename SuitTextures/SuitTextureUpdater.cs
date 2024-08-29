using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;

namespace SuitTextures
{
    internal class SuitTextureUpdater : IInRoomCallbacks
    {
        public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            Plugin.Logger.LogDebug($"targetPlayer: {targetPlayer.NickName} changedProps: {changedProps}");
            string _changedSuitTexture = null!;
            if (changedProps.ContainsKey("Suit texture"))
            {
                _changedSuitTexture = (string)changedProps["Suit texture"];
            }
            else return;
            if (!targetPlayer.IsLocal)
            {
                Player _cwPlayer = PlayerHandler.instance.players.First((player) => player.photonView.Owner == targetPlayer);
                Plugin.ApplySuitTexture(_cwPlayer, _changedSuitTexture);
            }
        }

        public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) { }
        public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {}
        public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {}
        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {}
    }
}
