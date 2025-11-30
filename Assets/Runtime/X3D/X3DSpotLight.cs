using UnityEngine;

namespace X3D {
public class X3DSpotLight : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // SpotLight fields
    public Color color = Color.white;
    public float intensity = 1.0f;
    public Vector3 location = Vector3.zero;
    public float radius = 100.0f;
    public float ambientIntensity = 0.0f;
    public Vector3 attenuation = new Vector3(1,0,0);
    public float beamWidth = 0.785398f;
    public float cutOffAngle = 1.5708f;
    public Vector3 direction = Vector3.forward;
    public bool on = true;
    public float shadowIntensity = 0.0f;
    public bool global = true;
}
}
