using UnityEngine;
namespace X3D {
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class X3DTriangleSet : MonoBehaviour {
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
            if (points != null && coordIndex != null && coordIndex.Length >= 3) {
                verts.AddRange(points);
                tris = coordIndex;
            } else {
                foreach (Transform child in transform) {
                    verts.Add(child.position - transform.position);
                }
                int triCount = verts.Count / 3;
                tris = new int[triCount * 3];
                for (int i = 0; i < triCount; i++) {
                    tris[i * 3 + 0] = i * 3 + 0;
                    tris[i * 3 + 1] = i * 3 + 1;
                    tris[i * 3 + 2] = i * 3 + 2;
                }
            }
            if (verts.Count < 3) return;
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