using UnityEngine;

namespace X3D {
public class X3DViewpoint : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;

    // Metadata
    public string metadata;

    // X3D 4.0 Viewpoint fields
    public Vector3 position = Vector3.zero;
    public Quaternion orientation = Quaternion.identity;
    public float fieldOfView = 0.785398f; // Default: 45 degrees in radians
    public string description;
    public bool jump = true;
    public bool retainUserOffsets = false;
    public Vector3 centerOfRotation = Vector3.zero;
    // Add any additional fields as needed for full X3D 4.0 compliance
}
}
