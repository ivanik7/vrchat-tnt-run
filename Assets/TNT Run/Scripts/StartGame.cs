
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class StartGame : UdonSharpBehaviour
{
    public ArenaManager arenaManager;
    public MapContainer[] mapContainers;
    [UdonSynced]
    public int selectedMap = 0;
    
    public override void Interact () {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PrepareGame");
    }

    public void PrepareGame () {
        Debug.Log("Prepare game");

        GetSelectedMapContainer().arena.SetActive(true);

        arenaManager.mapContainer = GetSelectedMapContainer();
        arenaManager.PrepareGame();

        var teleportLocation = GetSelectedMapContainer().spawn.transform;
        Networking.LocalPlayer.TeleportTo(teleportLocation.position, teleportLocation.rotation);
    }

    MapContainer GetSelectedMapContainer() {
        return mapContainers[selectedMap];
    }
}
