
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FallingCube : UdonSharpBehaviour
{
    void FixedUpdate() {
        if (transform. transform.localPosition.y <= -6f) {
            Destroy(transform.parent.gameObject);
        }
    }
}
