using UnityEngine;

namespace X3D {
public class X3DTransform : MonoBehaviour
{
    public Vector3 translation;
    public Quaternion rotation;
    public Vector3 scale;
    public Vector3 center;
    public Quaternion scaleOrientation;
    public string metadata;
    public string DEF;
    public string USE;
    public string containerField;
}
}
