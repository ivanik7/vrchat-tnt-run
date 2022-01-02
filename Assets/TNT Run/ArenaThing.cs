
using UdonSharp;
using UnityEngine;
using System.Collections;

public class ArenaThing : UdonSharpBehaviour
{
    readonly Vector3[] voxelVerts = new Vector3[] {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 2.0f, 0.0f),
        new Vector3(0.0f, 2.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 2.0f, 1.0f),
        new Vector3(0.0f, 2.0f, 1.0f),
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
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };


    public int width = 28;
    public int heght = 100;

    public Texture2D arenaMask;

    bool[] map;
    Vector3[] vertices;
    int[] triangles;

    Vector3[] verticesCollision;
    int[] trianglesCollision;
    
    Mesh mesh;
    Mesh collisionMesh;
    MeshCollider meshCollider; 

    void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = mesh;

        collisionMesh = new Mesh();
        collisionMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshCollider = GetComponent<MeshCollider>();

        map = new bool[width * heght];

        bool last = false;
        for (int i = 0; i < map.Length; i++)
        {
            // map[i] = last;
            // last = !last;
            map[i] = true;
        }

        if (arenaMask == null) {
            Debug.Log("pizdec");
        }

        GenerateMesh();
        GenerateCollisionMesh();
        float d = Time.realtimeSinceStartup;

        meshCollider.sharedMesh = collisionMesh;
        Debug.Log($"{Time.realtimeSinceStartup - d}");

    }

    void FixedUpdate()
    {
        float a = Time.realtimeSinceStartup;
        int x = Random.Range(0, width);
        int y = Random.Range(0, heght);
        
        for (int i = 0; i < 36; i++)
        {
            triangles[(x * heght + y) * 36 + i] = 0;
        }

        for (int i = 0; i < 6; i++)
        {
            trianglesCollision[(x * heght + y) * 6 + i] = 0;
        }

        float b = Time.realtimeSinceStartup;
        mesh.triangles = triangles;
        collisionMesh.triangles = trianglesCollision;
        float c = Time.realtimeSinceStartup;
        
        meshCollider.sharedMesh = collisionMesh;

        float d = Time.realtimeSinceStartup;

        Debug.Log($"{b-a}  {c-b}  {d-c}");
                // float d = Time.realtimeSinceStartup;

        // meshCollider.sharedMesh = collisionMesh;
        // Debug.Log($"{Time.realtimeSinceStartup - d}");
    }

    void GenerateMesh()
    {
        int blocks = 0;

        foreach (bool blockFlag in map)
        {
            if (blockFlag) blocks++;
        }

        Debug.Log($"Blocks {blocks}");

        vertices = new Vector3[blocks * 24];
        int verticesPtr = 0;
        triangles = new int[blocks * 36];
        int trisPtr = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < heght; y++)
            {
                if (map[x * heght + y])
                {
                    var pos = new Vector3(x, 0.0f, y);
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
                    }
                }
            }
        }

        Debug.Log($"Tris {trisPtr}/{triangles.Length}");
        Debug.Log($"Vertices {verticesPtr}/{vertices.Length}");

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
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
