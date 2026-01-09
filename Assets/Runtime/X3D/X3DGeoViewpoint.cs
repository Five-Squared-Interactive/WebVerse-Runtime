using UnityEngine;

namespace X3D {
public class X3DGeoViewpoint : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // GeoViewpoint fields
    public string[] geoSystem;
    public string geoCoords;
    public Quaternion orientation = Quaternion.identity;
    public float fieldOfView = 0.785398f; // default 45 degrees in radians
    public string description;
    public bool jump = true;
    public bool retainUserOffsets = false;
    public Vector3 centerOfRotation = Vector3.zero;
}
}
