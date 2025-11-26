using UnityEngine;
namespace X3D {
    public class X3DPointSet : MonoBehaviour {
    public float pointSize = 0.05f;
    public Color pointColor = Color.yellow;
    // X3D optional fields
    public Color[] colors;
    public Vector3[] points;
    public string metadata;
    public string DEF;
    public string USE;
    public string containerField;

        void OnDrawGizmos() {
            Gizmos.color = pointColor;
            if (points != null)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    if (colors != null && i < colors.Length)
                        Gizmos.color = colors[i];
                    else
                        Gizmos.color = pointColor;
                    Gizmos.DrawSphere(transform.TransformPoint(points[i]), pointSize);
                }
            }
            else
            {
                foreach (Transform child in transform) {
                    Gizmos.DrawSphere(child.position, pointSize);
                }
            }
        }
    }
}