using UnityEngine;

namespace X3D {
public class X3DParticleSystem : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // ParticleSystem fields
    public Color color = Color.white;
    public float size = 1.0f;
    public float lifetime = 1.0f;
    public float rate = 10.0f;
    public Vector3 direction = Vector3.up;
    public float speed = 1.0f;
    public float variation = 0.0f;
    public float mass = 1.0f;
    public float surfaceArea = 1.0f;
}
}
