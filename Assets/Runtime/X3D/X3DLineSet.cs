using UnityEngine;
namespace X3D {
    public class X3DLineSet : MonoBehaviour {
    public Color lineColor = Color.green;
    // X3D optional fields
    public Color[] colors;
    public Vector3[] points;
    public int[] colorIndex;
    public int[] coordIndex;
    public string metadata;
    public string DEF;
    public string USE;
    public string containerField;

        void OnDrawGizmos() {
            if (points != null && coordIndex != null && coordIndex.Length >= 2)
            {
                for (int i = 0; i < coordIndex.Length - 1; i += 2)
                {
                    int idxA = coordIndex[i];
                    int idxB = coordIndex[i + 1];
                    if (idxA < points.Length && idxB < points.Length)
                    {
                        if (colors != null && colorIndex != null && i / 2 < colorIndex.Length && colorIndex[i / 2] < colors.Length)
                            Gizmos.color = colors[colorIndex[i / 2]];
                        else
                            Gizmos.color = lineColor;
                        Gizmos.DrawLine(transform.TransformPoint(points[idxA]), transform.TransformPoint(points[idxB]));
                    }
                }
            }
            else
            {
                Gizmos.color = lineColor;
                Transform prev = null;
                foreach (Transform child in transform) {
                    if (prev != null)
                        Gizmos.DrawLine(prev.position, child.position);
                    prev = child;
                }
            }
        }
    }
}