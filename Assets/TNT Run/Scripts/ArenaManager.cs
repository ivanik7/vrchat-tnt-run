
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
    public Material[] fallingCubeMaterials;

    public MapContainer mapContainer;
    int[] layersHeight;

    CircularBufferVector3Int buffer;

    readonly Vector2[] playerBoundaries = new Vector2[] {
        new Vector2(-1f, -1f),
        new Vector2(1f, -1f),
        new Vector2(1f, 1f),
        new Vector2(-1f, 1f),
    };

    public float playerSize = 0.1f;
    public int removeDelay = 18;
    public bool gameStarted = false;
    void Start()
    {
        buffer = GetComponent<CircularBufferVector3Int>();

        localPlayer = Networking.LocalPlayer;
    }

    public void PrepareGame()
    {
        layersHeight = mapContainer.layersHeight;
        
        for (int i = 0; i < mapContainer.layersHeight.Length; i++)
        {
            var plane = arenaPlanes[i];
            plane.gameObject.SetActive(true);
            plane.transform.localPosition = new Vector3(0, mapContainer.layersHeight[i], 0);
            plane.Init();
            plane.mapContainer = mapContainer; 
            plane.FillMap();
        }

        for (int i = mapContainer.layersHeight.Length; i < arenaPlanes.Length; i++)
        {
            arenaPlanes[i].gameObject.SetActive(false);
        }

        // gameStarted = true;
    }

    void FixedUpdate()
    {
        if (gameStarted)
        {
            if (localPlayer != null && localPlayer.IsPlayerGrounded())
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
                            lastBlock = blockPos;
                            buffer.Add(blockPos);
                        }

                        break;
                    }
                }
            } else {
                buffer.Add(new Vector3Int(0, -1, 0));
            }

            while (buffer.GetLength() > removeDelay)
            {
                var pos = buffer.Peek();
                if (pos.y != -1) {
                    var isDeleted = ProcessBlock(arenaPlanes[pos.y], new Vector2Int(pos.x, pos.z));
                    if (isDeleted )
                    {
                        localPlayerState.Add(pos);
                    }
                }
            }

            localPlayerState.SendIfNotEmpty();
        }
    }

    bool ProcessBlock(ArenaPlane plane, Vector2Int pos)
    {
        var isBlockDeleted = plane.RemoveBlock(pos);

        if (isBlockDeleted && localPlayer.GetPosition().y > plane.transform.position.y) {
            var fallingCubeInstance = VRCInstantiate(fallingCube);
            fallingCubeInstance.transform.SetParent(plane.transform);
            fallingCubeInstance.transform.position = plane.transform.TransformPoint(new Vector3(pos.x, 0, pos.y));
            fallingCubeInstance.transform.localScale = new Vector3(1, 1, 1);
            fallingCubeInstance.GetComponentInChildren<MeshRenderer>().material = fallingCubeMaterials[ColorToTextureId(mapContainer.texture.GetPixel(pos.x, pos.y))];
        }

        return isBlockDeleted;
    }

    int ColorToTextureId(Color c)
    {
        if (c.r > 0) return 0;
        if (c.g > 0) return 1;
        return 2;
    }

    public void ApplyNetworkData(Vector3[] networkBufer, int networkBuferPtr) {
        for (int i = 0; i < networkBuferPtr; i++)
        {
            ProcessBlock(arenaPlanes[(int)networkBufer[i].y], new Vector2Int((int)networkBufer[i].x, (int)networkBufer[i].z));
        }
    }
}
