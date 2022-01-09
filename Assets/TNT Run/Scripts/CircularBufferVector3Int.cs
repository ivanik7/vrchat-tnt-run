
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class CircularBufferVector3Int : UdonSharpBehaviour
{
    public int size = 20;
    Vector3Int[] buffer;
    int headPtr = 0;
    int tailPtr = 0;

    void Start() {
        buffer = new Vector3Int[size];
    }

    public void Add(Vector3Int obj)
    {
        headPtr++;
        if (headPtr >= buffer.Length) {
            headPtr = 0;
        }

        buffer[headPtr] = obj;
    }

    public Vector3Int Peek()
    {
        tailPtr++;

        if (tailPtr >= buffer.Length) {
            tailPtr = 0;
        }

        return buffer[tailPtr];
    }

        public int GetLength() {
        if (tailPtr > headPtr) {
            return headPtr + buffer.Length - 1 - tailPtr;
        }

        return headPtr - tailPtr;
    }
}