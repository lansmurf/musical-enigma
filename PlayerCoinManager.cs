using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; // Defines which Hashtable to use

namespace CoinMod
{
    public class PlayerCoinManager : MonoBehaviourPun, IInRoomCallbacks 
    {
        public int SharedCoins { get; private set; }

        #region Unity Lifecycle
        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_Client_RequestSync), RpcTarget.MasterClient);
                CoinPlugin.Log.LogInfo("Client has started. Requesting coin sync from host.");
            }
        }
        #endregion
        
        #region Public Method for Other Scripts
        public void RequestModifyCoins(int amount)
        {
            photonView.RPC(nameof(RPC_Host_ProcessCoinModification), RpcTarget.MasterClient, amount);
        }
        #endregion

        #region RPCs for Networking
        [PunRPC]
        private void RPC_Host_ProcessCoinModification(int amount)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            int newTotal = SharedCoins + amount;
            if (newTotal < 0) newTotal = 0;
            photonView.RPC(nameof(RPC_Client_UpdateCoins), RpcTarget.All, newTotal);
        }

        [PunRPC]
        private void RPC_Client_UpdateCoins(int newTotal)
        {
            SharedCoins = newTotal;
            if (CoinUI.Instance != null)
            {
                CoinUI.Instance.UpdateCoinCount(newTotal);
            }
            CoinPlugin.Log.LogInfo($"Coin count for {photonView.Owner.NickName} updated to {newTotal}");
        }

        [PunRPC]
        private void RPC_Client_RequestSync(PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            photonView.RPC(nameof(RPC_Client_UpdateCoins), info.Sender, SharedCoins);
            CoinPlugin.Log.LogInfo($"Host received sync request. Sending {SharedCoins} coins to {info.Sender.NickName}.");
        }
        #endregion
        
        #region IInRoomCallbacks Implementation (The Fix)
        
        // We are now using the full, explicit type names for each parameter
        // to remove any possible confusion for the compiler.
        
        public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_Client_UpdateCoins), newPlayer, SharedCoins);
                CoinPlugin.Log.LogInfo($"New player {newPlayer.NickName} joined. Host is sending them the current coin count: {SharedCoins}.");
            }
        }

        public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) { }
        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }
        public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps) { }
        public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) { }
        #endregion
    }
}