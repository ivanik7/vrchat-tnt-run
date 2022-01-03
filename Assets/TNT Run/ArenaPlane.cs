
using UdonSharp;
using UnityEngine;
using System.Collections;

public class ArenaPlane : UdonSharpBehaviour
{
    readonly Vector3[] voxelVerts = new Vector3[] {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };

    readonly int[] voxelTris = new int[] {

		0, 3, 1, 2, // Back Face
		5, 6, 4, 7, // Front Face
		3, 7, 2, 6, // Top Face
		1, 5, 0, 4, // Bottom Face
		4, 7, 0, 3, // Left Face
		1, 2, 5, 6  // Right Face
	};

    readonly Vector2[] voxelUvs = new Vector2[] {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 0.25f),
        new Vector2 (0.25f, 0.0f),
        new Vector2 (0.25f, 0.25f)
    };

    public int width = 28;
    public int heght = 100;

    public Texture2D arenaMask;

    bool[] map;
    Vector3[] vertices;
    int[] triangles;

    Vector3[] verticesCollision;
    int[] trianglesCollision;

    int[] offsets;
    int[] offsetsCollsison;
    
    Mesh mesh;
    Mesh collisionMesh;
    MeshCollider meshCollider; 
    bool needMeshUpdate = false;

    void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = mesh;

        collisionMesh = new Mesh();
        collisionMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshCollider = GetComponent<MeshCollider>();
    }

    void FixedUpdate()
    {
        if (needMeshUpdate) {
            needMeshUpdate = false;

            float b = Time.realtimeSinceStartup;
            mesh.triangles = triangles;
            collisionMesh.triangles = trianglesCollision;
            float c = Time.realtimeSinceStartup;
            
            meshCollider.sharedMesh = collisionMesh;

            float d = Time.realtimeSinceStartup;
        }
    }

    public void FillMap() {
        map = new bool[width * heght];

        bool last = false;
        for (int i = 0; i < map.Length; i++)
        {
            // map[i] = last;
            // last = !last;
            map[i] = true;
        }

        GenerateMesh();
        GenerateCollisionMesh();
        float d = Time.realtimeSinceStartup;

        meshCollider.sharedMesh = collisionMesh;
        Debug.Log($"{Time.realtimeSinceStartup - d}");
    }

    public bool RemoveBlock(Vector2Int pos) {
        int x = pos.x;
        int y = pos.y;
        
        if (x < 0 || x > width || y < 0 || y > heght || !map[x * heght + y]) {
            return false;
        }

        map[x * heght + y] = false;

        int offset = offsets[x * heght + y];
        for (int i = 0; i < 36; i++)
        {
            triangles[offset * 36 + i] = 0;
        }

        for (int i = 0; i < 6; i++)
        {
            trianglesCollision[offset * 6 + i] = 0;
        }

        needMeshUpdate = true;

        Debug.Log($"Remove block {pos.x} {pos.y}");

        return true;
    }

    void GenerateMesh()
    {
        int blocks = 0;

        foreach (bool blockFlag in map)
        {
            if (blockFlag) blocks++;
        }

        Debug.Log($"Blocks {blocks}");

        offsets = new int[heght * width];
        int offset = 0; 
        vertices = new Vector3[blocks * 24];
        int verticesPtr = 0;
        triangles = new int[blocks * 36];
        int trisPtr = 0;
        var uv = new Vector2[blocks * 24];
        int uvPtr = 0;
        var uvOffset = new Vector2(0f, 0.75f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < heght; y++)
            {
                if (map[x * heght + y])
                {
                    var pos = new Vector3(x, 0.0f, y);
                    offsets[x * heght + y] = offset++;

                    for (int p = 0; p < 6; p++)
                    {
                        triangles[trisPtr++] = (verticesPtr);
                        triangles[trisPtr++] = (verticesPtr + 1);
                        triangles[trisPtr++] = (verticesPtr + 2);
                        triangles[trisPtr++] = (verticesPtr + 2);
                        triangles[trisPtr++] = (verticesPtr + 1);
                        triangles[trisPtr++] = (verticesPtr + 3);
                        vertices[verticesPtr++] = (pos + voxelVerts[voxelTris[p * 4 + 0]]);
                        vertices[verticesPtr++] = (pos + voxelVerts[voxelTris[p * 4 + 1]]);
                        vertices[verticesPtr++] = (pos + voxelVerts[voxelTris[p * 4 + 2]]);
                        vertices[verticesPtr++] = (pos + voxelVerts[voxelTris[p * 4 + 3]]);
                        uv[uvPtr++] = voxelUvs[0] + uvOffset;
                        uv[uvPtr++] = voxelUvs[1] + uvOffset;
                        uv[uvPtr++] = voxelUvs[2] + uvOffset;
                        uv[uvPtr++] = voxelUvs[3] + uvOffset;
                    }
                }
            }
        }

        Debug.Log($"Tris {trisPtr}/{triangles.Length}");
        Debug.Log($"Vertices {verticesPtr}/{vertices.Length}");

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
    }

    void GenerateCollisionMesh()
    {
        int blocks = 0;

        foreach (bool blockFlag in map)
        {
            if (blockFlag) blocks++;
        }

        Debug.Log($"Blocks {blocks}");

        verticesCollision = new Vector3[blocks * 4];
        int verticesPtr = 0;
        trianglesCollision = new int[blocks * 6];
        int trisPtr = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < heght; y++)
            {
                if (map[x * heght + y])
                {
                    var pos = new Vector3(x, 0.0f, y);
                    trianglesCollision[trisPtr++] = (verticesPtr);
                    trianglesCollision[trisPtr++] = (verticesPtr + 1);
                    trianglesCollision[trisPtr++] = (verticesPtr + 2);
                    trianglesCollision[trisPtr++] = (verticesPtr + 2);
                    trianglesCollision[trisPtr++] = (verticesPtr + 1);
                    trianglesCollision[trisPtr++] = (verticesPtr + 3);
                    verticesCollision[verticesPtr++] = (pos + voxelVerts[voxelTris[8 + 0]]);
                    verticesCollision[verticesPtr++] = (pos + voxelVerts[voxelTris[8 + 1]]);
                    verticesCollision[verticesPtr++] = (pos + voxelVerts[voxelTris[8 + 2]]);
                    verticesCollision[verticesPtr++] = (pos + voxelVerts[voxelTris[8 + 3]]);
                }
            }
        }

        Debug.Log($"Tris {trisPtr}/{trianglesCollision.Length}");
        Debug.Log($"Vertices {verticesPtr}/{verticesCollision.Length}");

        collisionMesh.Clear();
        collisionMesh.vertices = verticesCollision;
        collisionMesh.triangles = trianglesCollision;
    }
}
