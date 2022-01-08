
using UdonSharp;
using UnityEngine;
using System.Collections;

public class ArenaPlane : UdonSharpBehaviour
{
    public MapContainer mapContainer;
    int width;
    int height;
    bool[] map;

    int[] offsets;
    Mesh drawMesh;
    Mesh collisionMesh;
    MeshCollider meshCollider;
    int[] drawTriangles;
    int[] collisionTriangles;
    bool needMeshUpdate = false;


    void Start()
    {
        drawMesh = new Mesh();
        drawMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = drawMesh;

        collisionMesh = new Mesh();
        collisionMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshCollider = GetComponent<MeshCollider>();
    }

    void FixedUpdate()
    {
        if (needMeshUpdate) {
            needMeshUpdate = false;

            drawMesh.triangles = drawTriangles;
            collisionMesh.triangles = collisionTriangles;
            
            meshCollider.sharedMesh = collisionMesh;
        }
    }

    public void FillMap() {
        float d = Time.realtimeSinceStartup;

        width = mapContainer.texture.width;
        height = mapContainer.texture.height;
        
        map = (bool[])mapContainer.map.Clone();
        offsets = mapContainer.offsets;

        drawMesh.Clear();
        drawMesh.vertices = (Vector3[])mapContainer.drawMesh.vertices.Clone();
        drawTriangles = (int[])mapContainer.drawMesh.triangles.Clone();
        drawMesh.triangles = drawTriangles;
        drawMesh.uv = (Vector2[])mapContainer.drawMesh.uv.Clone();
        drawMesh.RecalculateNormals();

        collisionMesh.Clear();
        collisionMesh.vertices = (Vector3[])mapContainer.collisionMesh.vertices.Clone();
        collisionTriangles = (int[])mapContainer.collisionMesh.triangles.Clone();
        collisionMesh.triangles = collisionTriangles;

        meshCollider.sharedMesh = collisionMesh;

        Debug.Log($"{Time.realtimeSinceStartup - d}");
    }

    public bool RemoveBlock(Vector2Int pos) {
        int x = pos.x;
        int y = pos.y;

        if (x < 0 || x > width || y < 0 || y > height || !map[y * width + x]) {
            return false;
        }

        map[y * width + x] = false;

        int offset = offsets[y * width + x];
        for (int i = 0; i < 36; i++)
        {
            drawTriangles[offset * 36 + i] = 0;
        }

        for (int i = 0; i < 6; i++)
        {
            collisionTriangles[offset * 6 + i] = 0;
        }

        needMeshUpdate = true;

        return true;
    }
}
