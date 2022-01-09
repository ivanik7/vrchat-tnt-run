
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cyan.PlayerObjectPool;

public class PlayerStateEvenetListener : UdonSharpBehaviour
{
    public ArenaManager arenaManager;
    public CyanPlayerObjectAssigner objectPool;
    
    
    [PublicAPI]
    public void _OnLocalPlayerAssigned()
    {
        Debug.Log("The local player has been assigned an object from the pool!");
        
        var localPoolObject = (PlayerState)objectPool._GetPlayerPooledUdon(Networking.LocalPlayer);
        arenaManager.localPlayerState = localPoolObject;
    }

    [PublicAPI, HideInInspector]
    public VRCPlayerApi playerAssignedPlayer;
    [PublicAPI, HideInInspector]
    public int playerAssignedIndex;
    [PublicAPI, HideInInspector]
    public PlayerState playerAssignedPoolObject;
    [PublicAPI]
    public void _OnPlayerAssigned()
    {

        playerAssignedPoolObject.arenaManager = arenaManager;
    }

    [PublicAPI, HideInInspector]
    public VRCPlayerApi playerUnassignedPlayer;
    [PublicAPI, HideInInspector]
    public int playerUnassignedIndex;
    [PublicAPI, HideInInspector]
    public UdonBehaviour playerUnassignedPoolObject;
    [PublicAPI]
    public void _OnPlayerUnassigned()
    {

    }
}
