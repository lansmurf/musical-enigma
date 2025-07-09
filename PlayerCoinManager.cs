using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace CoinMod
{
    public class PlayerCoinManager : MonoBehaviourPun, IInRoomCallbacks 
    {
        public static PlayerCoinManager LocalInstance { get; private set; }

        public int SharedCoins { get; private set; }

        #region Unity Lifecycle

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
            
            var hostManager = LocalInstance;
            if (hostManager == null)
            {
                CoinPlugin.Log.LogError("Host's local PlayerCoinManager not found! Coin modification failed.");
                return;
            }

            int newTotal = hostManager.SharedCoins + amount;
            if (newTotal < 0) newTotal = 0;
            
            hostManager.photonView.RPC(nameof(RPC_Client_UpdateCoins), RpcTarget.All, newTotal);
        }
        
        [PunRPC]
        private void RPC_Host_ProcessPurchaseRequest(string itemName, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var hostManager = LocalInstance;
            if (hostManager == null)
            {
                CoinPlugin.Log.LogError("Host's local PlayerCoinManager not found! Purchase failed.");
                return;
            }

            if (ShopDatabase.ItemData.TryGetValue(itemName, out var itemData))
            {
                if (hostManager.SharedCoins >= itemData.Price)
                {
                    int newTotal = hostManager.SharedCoins - itemData.Price;
                    hostManager.photonView.RPC(nameof(RPC_Client_UpdateCoins), RpcTarget.All, newTotal);
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
            // This RPC is received by the HOST's PlayerCoinManager component on ALL clients.
            // We must find the LOCAL player's manager instance and update ITS value.
            if (LocalInstance != null)
            {
                // This updates the correct manager on each client's machine.
                LocalInstance.SharedCoins = newTotal;
            }

            // The UI instances are global singletons, so this part is correct and
            // will update the UI for the local player on their machine.
            if (CoinUI.Instance != null)
            {
                CoinUI.Instance.UpdateCoinCount(newTotal);
            }

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.RefreshShopDisplay();
            }
            
            CoinPlugin.Log.LogInfo($"Team coin count has been updated to {newTotal}");
        }

        [PunRPC]
        private void RPC_Client_RequestSync(PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

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