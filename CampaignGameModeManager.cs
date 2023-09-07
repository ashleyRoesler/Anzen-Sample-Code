using MLAPI.NetworkedVar;
using MLAPI.Messaging;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;
using ReimEnt.Core;
using UnityEngine.UI;

public class CampaignGameModeManager : GenericGameModeManager<CampaignGameMode> {

    private static readonly Logger _logger = new Logger("Campaign");

    public Text ReviveTimerText;

    [SuffixLabel("second(s)")]
    public float ReviveDelay = 3f;
    private float _reviveTimer = 0f;

    public event Action<CampaignState> StateChanged;
    public event Action Started;

    [HideInEditorMode]
    public CampaignState State {
        get => _state; set {
            var old = _state;
            _state = value;

            if (old != _state)
                OnStateChanged(old, _state);
        }
    }
    private CampaignState _state = CampaignState.Lobby;

    public bool UseLobbyTimer = false;

    [SyncedVar]
    [HideInEditorMode, ReadOnly]
    public float RemainingLobbyTime = 30f;

    [Space]
    public GameObject PilotPrefab;

    protected override void OnInitialize(CampaignGameMode gameMode, Level level) {
        base.OnInitialize(gameMode, level);

        if (IsServer)
            State = CampaignState.Lobby;

        Team.CurrentTeams = gameMode.Teams;
    }

    protected void Update() {

        if (IsClient && Player.Local != null)
            Player.Local.Character.enabled = State == CampaignState.OnGoing;

        if (IsServer) {

            if (State == CampaignState.Lobby && UseLobbyTimer) {

                if (Network.ClientCount >= 2)
                    RemainingLobbyTime -= Time.deltaTime;

                if (RemainingLobbyTime <= 0f)
                    Begin();
            }
        }

        // update revive timer if reviving player
        if (_reviveTimer > 0f) {
            _reviveTimer -= Time.deltaTime;

            ReviveTimerText.text = Mathf.CeilToInt(_reviveTimer).ToString();

            if (_reviveTimer <= 0f) {
                _reviveTimer = 0f;
                ReviveTimerText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Begins the campaign (only ran on server)
    /// </summary>
    public async void Begin() {

        if (!IsServer)
            throw new Exception("Only server can run this");

        State = CampaignState.Transitioning;

        _logger.Log("Beginning campaign...");

        /// Load the level
        await Network.LoadLevel(Level);

        /// Place everyone at the spawn points
        var spawnPoints = FindObjectsOfType<SpawnPoint>();
        var players = FindObjectsOfType<Player>().ToList();
        foreach (SpawnPoint spawnPoint in spawnPoints) {
            foreach (Player player in players) {
                
                /// spawn player
                player.Character.Respawn(spawnPoint.transform.position, spawnPoint.transform.rotation);

                /// spawn Pilot
                if (player.HasPilot) {
                    SpawnPilot(player);
                }

                players.Remove(player);
                break;
            }
        }

        State = CampaignState.OnGoing;
    }

    public void RequestBegin() {
        InvokeServerRpc(Net_RequestBegin, NetChannels.FullyReliable);
    }

    protected GameObject[] GetObjectsToPersist() {
        List<GameObject> objects = new List<GameObject>();

        objects.AddRange(FindObjectsOfType<Player>().Select(m => m.gameObject));
        objects.Add(gameObject);
        objects.RemoveAll(go => go == null);

        return objects.ToArray();
    }

    private void OnStateChanged(CampaignState old, CampaignState state) {
        StateChanged?.Invoke(state);

        if (old == CampaignState.Lobby)
            Started?.Invoke();

        if (IsServer)
            InvokeClientRpcOnEveryoneExcept(Net_SetState, ServerId, state, NetChannels.FullyReliable);
    }

    public void RequestRevive(Player player) {
        StartCoroutine(ReviveInPlaceCoroutine(player));
    }

    public void RequestRespawn(Player player) {
        if (IsServer) {

            // revive at last checkpoint (for VS, this is at the beginnning of the level)
            /// (a more complicated checkpoint system will be put in place after the VS)
            var spawnpoint = FindObjectOfType<SpawnPoint>();

            if (spawnpoint) {
                player.Character.Respawn(spawnpoint.transform.position, spawnpoint.transform.rotation);
            }
            else {
                player.Character.Respawn(Vector3.zero, Quaternion.identity);
            }
        }
        else
            InvokeServerRpc(Net_RequestRespawn, player, NetChannels.FullyReliable);
    }

    private IEnumerator ReviveInPlaceCoroutine(Player player) {
        _reviveTimer = ReviveDelay;
        ReviveTimerText.text = ReviveDelay.ToString();
        ReviveTimerText.gameObject.SetActive(true);

        yield return new WaitUntil(() => _reviveTimer == 0f);

        if (IsServer)
            player.Character.Revive();
        else
            InvokeServerRpc(Net_RequestRevive, player, NetChannels.FullyReliable);
    }

    #region Pilot
    [DesignerMethod("Spawn Pilot")]
    protected void Designer_SpawnPilot() {
        RequestSpawnPilot();        
    }

    private void RequestSpawnPilot() {

        if (!IsServer) {
            InvokeServerRpc(Net_RequestSpawnPilot, NetChannels.FullyReliable);
            return;
        }

        foreach (Player player in Player.Active) {

            if (!player.HasPilot) {
                SpawnPilot(player);
                player.HasPilot = true;
            }
        }
    }

    private void SpawnPilot(Player player) {
        GameObject pilot = Network.Spawn(PilotPrefab, player.PilotTargetPosition.position);
        pilot.GetComponent<Pilot>().SetTarget(player);
    }
    #endregion

    #region Networking
    protected override void OnPlayerJoined(ulong clientId) {
        base.OnPlayerJoined(clientId);
        InvokeClientRpcOnClient(Net_SetState, clientId, State, NetChannels.FullyReliable);

        if (State != CampaignState.Lobby)
            Player.GetByID(clientId).Character.Respawn();
    }

    [ServerRPC(RequireOwnership = false)]
    protected void Net_RequestBegin() => Begin();

    [ClientRPC]
    protected void Net_SetState(CampaignState state) => State = state;

    [ServerRPC(RequireOwnership = false)]
    protected void Net_RequestRevive(Player player) => player.Character.Revive();

    [ServerRPC(RequireOwnership = false)]
    protected void Net_RequestRespawn(Player player) => RequestRespawn(player);

    [ClientRPC]
    protected void Net_SpawnPilot(Player player) => SpawnPilot(player);

    [ServerRPC(RequireOwnership = false)]
    protected void Net_RequestSpawnPilot() => RequestSpawnPilot();
    #endregion
}

public enum CampaignState {
    Transitioning, Lobby, OnGoing
}