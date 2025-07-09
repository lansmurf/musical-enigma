using Photon.Pun;
using UnityEngine;

namespace CoinMod
{
    // This component is attached to each Player object.
    public class PlayerCoinManager : MonoBehaviourPun
    {
        // This is the synchronized value. It will be updated by the host for everyone.
        public int SharedCoins { get; private set; }

        /// <summary>
        /// This is the public method that any client-side code (like Luggage or Shop) should call.
        /// It will send the request to the host.
        /// </summary>
        public void RequestModifyCoins(int amount)
        {
            // Use our own PhotonView to send an RPC, but target it specifically at the MasterClient.
            photonView.RPC(nameof(RPC_Host_ProcessCoinModification), RpcTarget.MasterClient, amount);
        }

        /// <summary>
        /// [RPC] This method ONLY runs on the MasterClient's machine.
        /// It receives requests from clients, calculates the new total, and then broadcasts it back.
        /// </summary>
        [PunRPC]
        private void RPC_Host_ProcessCoinModification(int amount)
        {
            // A safety check to ensure only the host ever runs this logic.
            if (!PhotonNetwork.IsMasterClient) return;

            int newTotal = SharedCoins + amount;
            if (newTotal < 0) newTotal = 0; // Prevent negative coins.

            // Now, the host uses its PhotonView to broadcast the final, authoritative result to everyone.
            photonView.RPC(nameof(RPC_Client_UpdateCoins), RpcTarget.All, newTotal);
        }

        /// <summary>
        /// [RPC] This method runs on EVERY player's machine (including the host).
        /// It receives the final coin total from the host and updates the local state.
        /// </summary>
        [PunRPC]
        private void RPC_Client_UpdateCoins(int newTotal)
        {
            SharedCoins = newTotal;

            // Update our UI to show the new value.
            if (CoinUI.Instance != null)
            {
                CoinUI.Instance.UpdateCoinCount(newTotal);
            }
            
            CoinPlugin.Log.LogInfo($"Coin count for {photonView.Owner.NickName} updated to {newTotal}");
        }
    }
}