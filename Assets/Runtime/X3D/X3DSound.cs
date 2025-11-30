using UnityEngine;

namespace X3D {
public class X3DSound : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // Sound fields
    public Vector3 direction = Vector3.forward;
    public float intensity = 1.0f;
    public Vector3 location = Vector3.zero;
    public float maxBack = 10.0f;
    public float maxFront = 10.0f;
    public float minBack = 1.0f;
    public float minFront = 1.0f;
    public float priority = 0.0f;
    public bool spatialize = false;
    public GameObject source;
}
}
