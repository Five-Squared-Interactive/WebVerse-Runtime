using UnityEngine;

namespace X3D {
public class X3DFog : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // Fog fields
    public Color color = Color.white;
    public string fogType = "LINEAR";
    public float visibilityRange = 0.0f;
}
}
