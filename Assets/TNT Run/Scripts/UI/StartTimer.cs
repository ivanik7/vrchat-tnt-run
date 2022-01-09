
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class StartTimer : UdonSharpBehaviour
{
    Text text;
    Animator animator;
    void Start() {
        text = GetComponent<Text>();
        animator = GetComponent<Animator>();
    }
    public void Display(int value) {
        text.text = value == 0 ? "Go!" : $"{value}";
        animator.SetTrigger("Fade");
    }
}
