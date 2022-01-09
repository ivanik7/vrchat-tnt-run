
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


public class GameManager : UdonSharpBehaviour
{
    public const int LOBBY = 0;
    public const int WAIT = 1;
    public const int STARTED = 2;
    public Transform lobbySpawn;
    public ArenaManager arenaManager;
    public MapContainer[] mapContainers;
    public StartTimer startTimer;
    [UdonSynced]
    public int selectedMap = 0;
    [UdonSynced]
    public int gameState = LOBBY;
    [UdonSynced]
    public double gameStartedAt = 0;
    public double lastTimerDisplay = 0;
    [UdonSynced]
    public int playersIngame = 0;
    [UdonSynced]
    public int playersFailed = 0;

    VRCPlayerApi localPlayer;
    void Start() {
        localPlayer = Networking.LocalPlayer;
    }
    
    void FixedUpdate () {
        if (Networking.IsOwner(gameObject)) {
            if (gameState == WAIT && Networking.GetServerTimeInSeconds() - lastTimerDisplay > 1f) {
                lastTimerDisplay = Networking.GetServerTimeInSeconds();
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DisplayTimer");
            }

            if (gameState == WAIT && Networking.GetServerTimeInSeconds() - gameStartedAt > 6f) {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartGame");
            }
        }

        if (localPlayer.GetPosition().y < -10f) {
            localPlayer.TeleportTo(lobbySpawn.position, lobbySpawn.rotation);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "PlayerFailed");
        }
    }

    public void UpdateValues() {
        RequestSerialization();
    }

    public void StartButton() {
        
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PrepareGame");
    }

    public void PrepareGame () {
        if (Networking.IsOwner(gameObject)) {
            gameState = WAIT;
            gameStartedAt = Networking.GetServerTimeInSeconds();
            playersIngame = VRCPlayerApi.GetPlayerCount();

            UpdateValues();
        }

        GetSelectedMapContainer().arena.SetActive(true);

        arenaManager.mapContainer = GetSelectedMapContainer();
        arenaManager.PrepareGame();

        var teleportLocation = GetSelectedMapContainer().spawn.transform;
        Networking.LocalPlayer.TeleportTo(teleportLocation.position, teleportLocation.rotation);
    }

    public void StartGame () {
        if (Networking.IsOwner(gameObject)) {
            gameState = STARTED;
            UpdateValues();
        }

        arenaManager.gameStarted = true;
    }

    public void EndGame() {
        if (Networking.IsOwner(gameObject)) {
            gameState = LOBBY;
            
            UpdateValues();
        }

        arenaManager.gameStarted = false;
    }

    public void DisplayTimer() {
        int timeToDisplay = (int)(gameStartedAt - Networking.GetServerTimeInSeconds() - 0.1);

        Debug.Log($"Timer {timeToDisplay}");
        startTimer.Display(6 + timeToDisplay);
    }

    public void PlayerFailed() {
        playersFailed++;

        if (playersFailed >= playersIngame) {
           SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "EndGame");
        } else {
            UpdateValues();
        }
    }

    MapContainer GetSelectedMapContainer() {
        return mapContainers[selectedMap];
    }
}
