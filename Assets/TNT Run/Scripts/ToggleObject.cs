
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleObject : UdonSharpBehaviour
{
    public GameObject toggleObject;
    override public void Interact() {
        toggleObject.SetActive(!toggleObject.activeSelf);
    }
}
