using Photon.Pun;
using UnityEngine;

namespace CoinMod
{
    // This component will be attached to the Player object, which is already networked and persistent.
    public class PlayerCoinManager : MonoBehaviourPun
    {
        // A static reference to the HOST's instance of this component.
        public static PlayerCoinManager HostInstance { get; private set; }

        public int SharedCoins { get; private set; }

        private void Awake()
        {
            // If this component is on the MasterClient's player object, it becomes the authority.
            if (photonView.IsMine && PhotonNetwork.IsMasterClient)
            {
                if (HostInstance != null)
                {
                    CoinPlugin.Log.LogWarning("Multiple HostInstances detected. This shouldn't happen.");
                }
                HostInstance = this;
                CoinPlugin.Log.LogInfo("This player is the host. PlayerCoinManager is now authoritative.");
            }
        }

        // Only the host can call this to change the value.
        // It's not an RPC itself.
        public void ModifyCoinsOnHost(int amount)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                CoinPlugin.Log.LogError("A client tried to call ModifyCoinsOnHost directly!");
                return;
            }

            int newTotal = SharedCoins + amount;
            if (newTotal < 0) newTotal = 0;

            // Now, the host broadcasts the new total to everyone.
            photonView.RPC(nameof(RPC_Client_UpdateCoins), RpcTarget.All, newTotal);
        }

        [PunRPC]
        private void RPC_Client_UpdateCoins(int newTotal)
        {
            // This runs on everyone's machine to keep the value synchronized.
            SharedCoins = newTotal;
        }
        
        // This is the RPC that clients will call.
        [PunRPC]
        public void RPC_Request_ModifyCoins(int amount)
        {
            if (HostInstance != null)
            {
                // The client's request is forwarded to the host's instance to be processed.
                HostInstance.ModifyCoinsOnHost(amount);
            }
        }
    }
}