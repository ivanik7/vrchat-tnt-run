using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PlayerState : UdonSharpBehaviour
{
    [PublicAPI, HideInInspector]
    public VRCPlayerApi Owner;

    public ArenaManager arenaManager;
    [UdonSynced] Vector3[] networkBufer;
    [UdonSynced] int networkBuferPtr;
    
    [PublicAPI]
    public void _OnOwnerSet()
    {
        if (Owner.isLocal) {
            Reset();
        }
    }

    [PublicAPI]
    public void _OnCleanup()
    {
    }

    public void Reset() {
        networkBufer = new Vector3[32];
        networkBuferPtr = 0;
    }

    public void Add(Vector3Int pos) {
        if (networkBuferPtr < networkBufer.Length) {
            networkBufer[networkBuferPtr++] = pos;
        } else {
            Debug.Log("Network buffer is full");
        }
    }

    public void SendIfNotEmpty() {
        if (networkBuferPtr > 0) {
            RequestSerialization();
        }
    }

    public override void OnPostSerialization (VRC.Udon.Common.SerializationResult result) {
        if (!result.success) {
            Debug.Log("Player state serialization failed");
        }

        Reset();
    }

    public override void OnDeserialization () {
        if (arenaManager) {
            arenaManager.ApplyNetworkData(networkBufer, networkBuferPtr);
        }
    }
}