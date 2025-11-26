using UnityEngine;

namespace X3D {
public class X3DDirectionalLight : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // DirectionalLight fields
    public Color color = Color.white;
    public float intensity = 1.0f;
    public Vector3 direction = new Vector3(0, 0, -1);
    public float ambientIntensity = 0.0f;
    public bool on = true;
    public float shadowIntensity = 0.0f;
    public bool global = true;
}
}
