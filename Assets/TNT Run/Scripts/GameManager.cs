
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


public class GameManager : UdonSharpBehaviour
{
    public const int LOBBY = 0;
    public const int WAIT = 1;
    public const int STARTED = 2;
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
    }

    public void UpdateValues() {
        RequestSerialization();
    }

    public void StartButton() {
        
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "PrepareGame");
    }

    public void PrepareGame () {
        gameState = WAIT;
        gameStartedAt = Networking.GetServerTimeInSeconds();
        UpdateValues();

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PrepareArena");
    }

    public void PrepareArena () {
        GetSelectedMapContainer().arena.SetActive(true);

        arenaManager.mapContainer = GetSelectedMapContainer();
        arenaManager.PrepareGame();

        var teleportLocation = GetSelectedMapContainer().spawn.transform;
        Networking.LocalPlayer.TeleportTo(teleportLocation.position, teleportLocation.rotation);
    }

    public void StartGame () {
        if (Networking.IsOwner(gameObject)) {
            gameState = STARTED;
        }

        arenaManager.gameStarted = true;
        // TODO: Останавливать игру при телепорте на спавн
    }

    public void DisplayTimer() {
        int timeToDisplay = (int)(gameStartedAt - Networking.GetServerTimeInSeconds() - 0.1);

        Debug.Log($"Timer {timeToDisplay}");
        startTimer.Display(6 + timeToDisplay);
    }

    MapContainer GetSelectedMapContainer() {
        return mapContainers[selectedMap];
    }
}
