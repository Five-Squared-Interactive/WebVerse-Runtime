using UnityEngine;

namespace X3D {
public class X3DPointLight : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // PointLight fields
    public Color color = Color.white;
    public float intensity = 1.0f;
    public Vector3 location = Vector3.zero;
    public float radius = 100.0f;
    public float ambientIntensity = 0.0f;
    public Vector3 attenuation = new Vector3(1,0,0);
    public bool on = true;
    public float shadowIntensity = 0.0f;
    public bool global = true;
}
}
