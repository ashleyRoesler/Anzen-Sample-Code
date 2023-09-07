using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using System.Text;
using System.Collections;

[System.Obsolete]
public class PlayerAcceptanceManager : MonoBehaviour {

    private void Awake() {
        NetworkingManager.OnSingletonReady += NetworkingManager_OnSingletonReady;
        CustomMessagingManager.OnUnnamedMessage += CustomMessagingManager_OnUnnamedMessage;
    }

    private void CustomMessagingManager_OnUnnamedMessage(ulong clientId, System.IO.Stream stream) {
        BitReader reader = new BitReader(stream);
        StringBuilder builder = reader.ReadString();
        string str = builder.ToString();

        Dialogs.Request(str);
    }

    private void NetworkingManager_OnSingletonReady() {
        NetworkingManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCheck;
    }

    private void ConnectionApprovalCheck(byte[] connectionData, ulong clientId, NetworkingManager.ConnectionApprovedDelegate callback) {

        bool inLobby = false;
        bool maxPlayersReached = true;

        string rejectionReason = "";

        // check if game is in lobby
        if (Room.GameMode) {
            if (Room.GameMode.name.ToLower().Contains("arena")) {
                var arenaManager = FindObjectOfType<ArenaGameModeManager>();
                inLobby = arenaManager && arenaManager.State == ArenaState.Lobby;
            }
            else if (Room.GameMode.name.ToLower().Contains("campaign")) {
                var campaignManager = FindObjectOfType<CampaignGameModeManager>();
                inLobby = campaignManager && campaignManager.State == CampaignState.Lobby;
            }

            // check if max player limit has been reached
            maxPlayersReached = Player.Active.Count >= Room.GameMode.MaxPlayers;
        }

        if (!inLobby)
            rejectionReason = "Game is already in progress.";
        else if (maxPlayersReached)
            rejectionReason = "Game is full.";

        bool approve = inLobby && !maxPlayersReached;

        // reject or accept connection
        /// MLAPI doesn't give us a good way to send a rejection message, so we have to send a custom message.
        if (!approve) {
            MLAPI.Serialization.BitStream bitStream = new MLAPI.Serialization.BitStream();
            BitWriter writer = new BitWriter(bitStream);

            writer.WriteString($"Failed to connect to server. {rejectionReason} Please try another server.");

            CustomMessagingManager.SendUnnamedMessage(clientId, bitStream);

            StartCoroutine(ConnectionRejectionCoroutine(callback));
        }
        else {
            callback(approve && NetworkingManager.Singleton.NetworkConfig.CreatePlayerPrefab, null, approve, null, null);
        }            
    }

    private IEnumerator ConnectionRejectionCoroutine(NetworkingManager.ConnectionApprovedDelegate callback) {

        /// If we call the callback immediately after sending the rejection message to the client, the client
        /// doesn't have enough time to process the message before being disconnected. As a not great workaround,
        /// we can use a coroutine to wait a little before disconnecting the client. This way, hopefully, the message
        /// goes through. This person was having the same issue: https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/issues/796

        yield return new WaitForSeconds(0.5f);
        callback(false, null, false, null, null);
    }

    private void OnDisable() {
        NetworkingManager.OnSingletonReady -= NetworkingManager_OnSingletonReady;
        CustomMessagingManager.OnUnnamedMessage -= CustomMessagingManager_OnUnnamedMessage;

        if (NetworkingManager.Singleton)
            NetworkingManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCheck;
    }
}