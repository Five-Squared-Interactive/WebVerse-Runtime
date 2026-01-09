using UnityEngine;
namespace X3D {
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class X3DQuadSet : MonoBehaviour {
    // X3D optional fields
    public Color[] colors;
    public Vector3[] points;
    public Vector3[] normals;
    public Vector2[] texCoords;
    public int[] colorIndex;
    public int[] coordIndex;
    public int[] normalIndex;
    public int[] texCoordIndex;
    public string metadata;
    public string DEF;
    public string USE;
    public string containerField;

        public void Start() {
            // If points and coordIndex are set, use them; otherwise, fallback to children
            var verts = new System.Collections.Generic.List<Vector3>();
            int[] tris = null;
            if (points != null && coordIndex != null && coordIndex.Length >= 4) {
                verts.AddRange(points);
                // Convert quad indices to triangles
                int quadCount = coordIndex.Length / 4;
                tris = new int[quadCount * 6];
                for (int i = 0; i < quadCount; i++) {
                    int vi = i * 4;
                    tris[i * 6 + 0] = coordIndex[vi + 0];
                    tris[i * 6 + 1] = coordIndex[vi + 1];
                    tris[i * 6 + 2] = coordIndex[vi + 2];
                    tris[i * 6 + 3] = coordIndex[vi + 0];
                    tris[i * 6 + 4] = coordIndex[vi + 2];
                    tris[i * 6 + 5] = coordIndex[vi + 3];
                }
            } else {
                foreach (Transform child in transform) {
                    verts.Add(child.position - transform.position);
                }
                int quadCount = verts.Count / 4;
                tris = new int[quadCount * 6];
                for (int i = 0; i < quadCount; i++) {
                    int vi = i * 4;
                    tris[i * 6 + 0] = vi + 0;
                    tris[i * 6 + 1] = vi + 1;
                    tris[i * 6 + 2] = vi + 2;
                    tris[i * 6 + 3] = vi + 0;
                    tris[i * 6 + 4] = vi + 2;
                    tris[i * 6 + 5] = vi + 3;
                }
            }
            if (verts.Count < 4) return;
            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            if (normals != null && normals.Length == verts.Count)
                mesh.SetNormals(new System.Collections.Generic.List<Vector3>(normals));
            if (texCoords != null && texCoords.Length == verts.Count)
                mesh.SetUVs(0, new System.Collections.Generic.List<Vector2>(texCoords));
            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}