
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class StartGame : UdonSharpBehaviour
{
    public ArenaManager arenaManager;
    
    public override void Interact () {
        arenaManager.PrepareGame();
    }
}
