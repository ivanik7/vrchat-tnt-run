
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ArenaManager : UdonSharpBehaviour
{
    VRCPlayerApi localPlayer;
    public PlayerState localPlayerState;

    public ArenaPlane[] arenaPlanes;
    public GameObject fallingCube;
    float[] layersHeight;

    CircularBufferVector3Int buffer;

    readonly Vector2[] playerBoundaries = new Vector2[] {
        new Vector2(-1f, -1f),
        new Vector2(1f, -1f),
        new Vector2(1f, 1f),
        new Vector2(-1f, 1f),
    };

    public float playerSize = 0.1f;
    public int removeDelay = 18;
    bool gameStarted = false;
    void Start()
    {
        buffer = GetComponent<CircularBufferVector3Int>();

        layersHeight = new float[arenaPlanes.Length];

        for (int i = 0; i < arenaPlanes.Length; i++)
        {
            layersHeight[i] = arenaPlanes[i].transform.position.y;
        }


        localPlayer = Networking.LocalPlayer;
    }

    public void PrepareGame()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ProcessPrepareGame");

    }

    public void ProcessPrepareGame()
    {
        foreach (var plane in arenaPlanes)
        {
            plane.FillMap();
        }

        gameStarted = true;
    }


    void FixedUpdate()
    {
        if (gameStarted)
        {
            if (localPlayer.IsPlayerGrounded())
            {
                var pos = transform.InverseTransformPoint(localPlayer.GetPosition());

                for (int i = 0; i < arenaPlanes.Length; i++)
                {
                    if (pos.y > layersHeight[i])
                    {
                        Vector3Int lastBlock = new Vector3Int(-1, -1, -1);
                        foreach (var bound in playerBoundaries)
                        {
                            var blockPos = new Vector3Int(Mathf.FloorToInt(pos.x + bound.x * playerSize), i, Mathf.FloorToInt(pos.z + bound.y * playerSize));
                            if (lastBlock != blockPos)
                            {
                                lastBlock = blockPos;
                                buffer.Add(blockPos);
                            }
                        }

                        break;
                    }
                }
            }

            while (buffer.GetLength() > removeDelay)
            {
                var pos = buffer.Peek();
                var isDeleted = ProcessBlock(arenaPlanes[pos.y], new Vector2Int(pos.x, pos.z));
                if (isDeleted )
                {
                    localPlayerState.Add(pos);
                }
            }

            localPlayerState.SendIfNotEmpty();
        }
    }

    bool ProcessBlock(ArenaPlane plane, Vector2Int pos)
    {
        var isBlockDeleted = plane.RemoveBlock(pos);

        if (isBlockDeleted) {
            var fallingCubeInstance = VRCInstantiate(fallingCube);
            fallingCubeInstance.transform.position = plane.transform.TransformPoint(new Vector3(pos.x, 0, pos.y));
            fallingCubeInstance.transform.SetParent(plane.transform);
        }

        return isBlockDeleted;
    }

    public void ApplyNetworkData(Vector3[] networkBufer, int networkBuferPtr) {
        for (int i = 0; i < networkBuferPtr; i++)
        {
            ProcessBlock(arenaPlanes[(int)networkBufer[i].y], new Vector2Int((int)networkBufer[i].x, (int)networkBufer[i].z));
        }
    }
}
