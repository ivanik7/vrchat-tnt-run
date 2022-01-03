
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class StartGame : UdonSharpBehaviour
{
    public ArenaManager arenaManager;
    public Transform arenaSpawn;
    
    public override void Interact () {
        arenaManager.PrepareGame();

        var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Teleport");
    }

    public void Teleport () {
        Debug.Log("teleport");
        Networking.LocalPlayer.TeleportTo(arenaSpawn.position, arenaSpawn.rotation);
    }
}
