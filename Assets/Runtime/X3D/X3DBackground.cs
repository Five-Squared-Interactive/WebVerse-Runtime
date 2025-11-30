using UnityEngine;

namespace X3D {
public class X3DBackground : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // Background fields
    public string[] skyColor;
    public string[] groundColor;
    public float[] skyAngle;
    public float[] groundAngle;
    public float transparency;
}
}
