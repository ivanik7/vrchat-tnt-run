using UnityEngine;
using UdonSharp;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

public class MapContainer : UdonSharpBehaviour
{
    public Texture2D texture;
    public Mesh drawMesh;
    public Mesh collisionMesh;
    [HideInInspector]
    public int[] offsets;
    [HideInInspector]
    public bool[] map;
}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
[CustomEditor(typeof(MapContainer))]
public class MapContainerEditor : Editor
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

    Vector2[] uvOffset = new Vector2[] {
        new Vector2(0f, 0.75f),
        new Vector2(0.25f, 0.75f),
        new Vector2(0.5f, 0.75f)
    };

    public override void OnInspectorGUI()
    {
        // Draws the default convert to UdonBehaviour button, program asset field, sync settings, etc.
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

        MapContainer container = (MapContainer)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Build mesh"))
        {
            var pixels = container.texture.GetPixels32();

            int blocks = 0;

            foreach (var pixel in pixels)
            {
                if (IsPixelSeted(pixel)) blocks++;
            }

            Debug.Log($"Blocks {blocks}");

            container.map = new bool[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                container.map[i] = IsPixelSeted(pixels[i]);
            }

            container.offsets = generateOffsets(pixels, container.texture.width, container.texture.height);
            container.drawMesh = GenerateMesh(pixels, container.texture.width, container.texture.height, blocks);
            container.collisionMesh = GenerateCollisionMesh(pixels, container.texture.width, container.texture.height, blocks);
        }
    }
    int[] generateOffsets(Color32[] pixels, int width, int height)
    {
        var offsets = new int[pixels.Length];
        int offset = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (IsPixelSeted(pixels[x * height + y]))
                {
                    var pos = new Vector3(x, 0.0f, y);
                    offsets[x * height + y] = offset++;
                }
            }
        }

        return offsets;
    }

    Mesh GenerateMesh(Color32[] pixels, int width, int height, int blocks)
    {
        var vertices = new Vector3[blocks * 24];
        int verticesPtr = 0;
        var triangles = new int[blocks * 36];
        int trisPtr = 0;

        var uv = new Vector2[blocks * 24];
        int uvPtr = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (IsPixelSeted(pixels[x * height + y]))
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

                        var pixel = pixels[x * height + y];
                        Vector2 blockUvOffset;
                        if (pixel.r > 0) blockUvOffset = uvOffset[0];
                        else if (pixel.g > 0) blockUvOffset = uvOffset[1];
                        else blockUvOffset = uvOffset[2];

                        uv[uvPtr++] = voxelUvs[0] + blockUvOffset;
                        uv[uvPtr++] = voxelUvs[1] + blockUvOffset;
                        uv[uvPtr++] = voxelUvs[2] + blockUvOffset;
                        uv[uvPtr++] = voxelUvs[3] + blockUvOffset;
                    }
                }
            }
        }

        Debug.Log($"Tris {trisPtr}/{triangles.Length}");
        Debug.Log($"Vertices {verticesPtr}/{vertices.Length}");

        var mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        return mesh;
    }

    Mesh GenerateCollisionMesh(Color32[] pixels, int width, int height, int blocks)
    {
        var vertices = new Vector3[blocks * 4];
        int verticesPtr = 0;
        var triangles = new int[blocks * 6];
        int trisPtr = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (IsPixelSeted(pixels[x * height + y]))
                {
                    var pos = new Vector3(x, 0.0f, y);
                    triangles[trisPtr++] = (verticesPtr);
                    triangles[trisPtr++] = (verticesPtr + 1);
                    triangles[trisPtr++] = (verticesPtr + 2);
                    triangles[trisPtr++] = (verticesPtr + 2);
                    triangles[trisPtr++] = (verticesPtr + 1);
                    triangles[trisPtr++] = (verticesPtr + 3);
                    vertices[verticesPtr++] = (pos + voxelVerts[voxelTris[8 + 0]]);
                    vertices[verticesPtr++] = (pos + voxelVerts[voxelTris[8 + 1]]);
                    vertices[verticesPtr++] = (pos + voxelVerts[voxelTris[8 + 2]]);
                    vertices[verticesPtr++] = (pos + voxelVerts[voxelTris[8 + 3]]);
                }
            }
        }

        Debug.Log($"Tris {trisPtr}/{triangles.Length}");
        Debug.Log($"Vertices {verticesPtr}/{vertices.Length}");

        var mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    bool IsPixelSeted(Color32 c)
    {
        return c.r > 0 || c.g > 0 || c.b > 0;
    }
}
#endif