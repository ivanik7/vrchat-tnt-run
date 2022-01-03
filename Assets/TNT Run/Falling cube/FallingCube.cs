
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FallingCube : UdonSharpBehaviour
{
    void FixedUpdate() {
        Debug.Log(transform.position.y);
        if (transform. transform.localPosition.y <= -6f) {
            Destroy(transform.parent.gameObject);
        }
    }
}
