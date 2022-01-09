
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class StartGame : UdonSharpBehaviour
{
    public GameManager gameManager;

    void Start() {
        if(!Networking.IsOwner(gameObject)) {
            gameObject.SetActive(false);
        }
    }
    public override void Interact () {
        gameManager.StartButton();
    }
}
