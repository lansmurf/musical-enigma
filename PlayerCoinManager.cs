using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
// We still use this alias for convenience, but will be explicit in the interface implementation.
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace CoinMod
{
    public class PlayerCoinManager : MonoBehaviourPun, IInRoomCallbacks 
    {
        // This static property will hold the instance of the PlayerCoinManager for the local player.
        // On the host's machine, this gives us an easy way to access the authoritative manager.
        public static PlayerCoinManager LocalInstance { get; private set; }

        public int SharedCoins { get; private set; }

        #region Unity Lifecycle

        // We add an Awake method to set the static LocalInstance.
        private void Awake()
        {
            if (photonView.IsMine)
            {
                LocalInstance = this;
            }
        }

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
        
        #region Public Methods
        public void RequestPurchase(string itemName)
        {
            photonView.RPC(nameof(RPC_Host_ProcessPurchaseRequest), RpcTarget.MasterClient, itemName);
        }

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
            
            // --- FIX ---
            // The RPC runs on the sender's PlayerCoinManager instance on the host machine.
            // We must find the HOST's own instance to get the authoritative coin count.
            var hostManager = LocalInstance;
            if (hostManager == null)
            {
                CoinPlugin.Log.LogError("Host's local PlayerCoinManager not found! Coin modification failed.");
                return;
            }

            int newTotal = hostManager.SharedCoins + amount;
            if (newTotal < 0) newTotal = 0;
            
            // Use the host's photonView to broadcast the new, correct total to everyone.
            hostManager.photonView.RPC(nameof(RPC_Client_UpdateCoins), RpcTarget.All, newTotal);
        }
        
        [PunRPC]
        private void RPC_Host_ProcessPurchaseRequest(string itemName, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // --- FIX ---
            // Same logic as above: find the host's authoritative manager.
            var hostManager = LocalInstance;
            if (hostManager == null)
            {
                CoinPlugin.Log.LogError("Host's local PlayerCoinManager not found! Purchase failed.");
                return;
            }

            if (ShopDatabase.ItemData.TryGetValue(itemName, out var itemData))
            {
                // Check against the HOST's coin count.
                if (hostManager.SharedCoins >= itemData.Price)
                {
                    int newTotal = hostManager.SharedCoins - itemData.Price;
                    
                    // Use the HOST's photonView to broadcast the coin update to everyone.
                    hostManager.photonView.RPC(nameof(RPC_Client_UpdateCoins), RpcTarget.All, newTotal);
                    
                    // Use the original photonView to send the confirmation only to the buyer. This is correct.
                    photonView.RPC(nameof(RPC_Client_ConfirmPurchase), info.Sender, itemName);
                    CoinPlugin.Log.LogInfo($"Host approved purchase of {itemName} for {info.Sender.NickName}. New coin total: {newTotal}");
                }
            }
        }
        
        [PunRPC]
        private void RPC_Client_ConfirmPurchase(string itemName)
        {
            if (ShopManager.Instance != null)
            {
                CoinPlugin.Log.LogInfo($"Purchase of {itemName} confirmed by host. Spawning item.");
                ShopManager.Instance.SpawnPurchasedItem(itemName);
            }
        }

        [PunRPC]
        private void RPC_Client_UpdateCoins(int newTotal)
        {
            SharedCoins = newTotal;
            
            if (CoinUI.Instance != null)
            {
                CoinUI.Instance.UpdateCoinCount(newTotal);
            }

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.RefreshShopDisplay();
            }
            
            // This log message can be a bit confusing, as it's the same on every client.
            // A more generic message might be better, but we'll keep it for now.
            CoinPlugin.Log.LogInfo($"Local coin count updated to {newTotal}");
        }

        [PunRPC]
        private void RPC_Client_RequestSync(PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // We use LocalInstance here as well for consistency, although `this` would also work
            // since this RPC is received on the host's own PlayerCoinManager.
            var hostManager = LocalInstance;
            if (hostManager == null) return;
            
            hostManager.photonView.RPC(nameof(RPC_Client_UpdateCoins), info.Sender, hostManager.SharedCoins);
            CoinPlugin.Log.LogInfo($"Host received sync request. Sending {hostManager.SharedCoins} coins to {info.Sender.NickName}.");
        }
        #endregion
        
        #region IInRoomCallbacks Implementation
        
        public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Use LocalInstance for consistency.
                var hostManager = LocalInstance;
                if (hostManager == null) return;

                hostManager.photonView.RPC(nameof(RPC_Client_UpdateCoins), newPlayer, hostManager.SharedCoins);
                CoinPlugin.Log.LogInfo($"New player {newPlayer.NickName} joined. Host is sending them the current coin count: {hostManager.SharedCoins}.");
            }
        }
        
        public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) { }
        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
        public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }
        public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) { }
        #endregion
    }
}