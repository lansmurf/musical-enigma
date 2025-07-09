using Photon.Pun;
using Photon.Realtime; // Required for Player and PhotonMessageInfo
using UnityEngine;

// Hashtable is in this namespace.
using ExitGames.Client.Photon; 

namespace CoinMod
{
    public class PlayerCoinManager : MonoBehaviourPun, IInRoomCallbacks 
    {
        public int SharedCoins { get; private set; }

        // --- Unity Lifecycle ---
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
        
        // --- Public Method for Other Scripts ---
        public void RequestModifyCoins(int amount)
        {
            photonView.RPC(nameof(RPC_Host_ProcessCoinModification), RpcTarget.MasterClient, amount);
        }

        // --- RPCs for Networking ---
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
        
        // --- IInRoomCallbacks Implementation ---
        
        /// <summary>This method is part of the IInRoomCallbacks contract.</summary>
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_Client_UpdateCoins), newPlayer, SharedCoins);
                CoinPlugin.Log.LogInfo($"New player {newPlayer.NickName} joined. Host is sending them the current coin count: {SharedCoins}.");
            }
        }

        /// <summary>This method is part of the IInRoomCallbacks contract.</summary>
        public void OnPlayerLeftRoom(Player otherPlayer) { }

        /// <summary>This method is part of the IInRoomCallbacks contract.</summary>
        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }

        /// <summary>This method is part of the IInRoomCallbacks contract.</summary>
        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }

        /// <summary>This method is part of the IInRoomCallbacks contract.</summary>
        public void OnMasterClientSwitched(Player newMasterClient) { }
    }
}