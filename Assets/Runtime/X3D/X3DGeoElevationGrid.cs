using UnityEngine;

namespace X3D {
public class X3DGeoElevationGrid : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // GeoElevationGrid fields
    public float[] height;
    public int xDimension;
    public int zDimension;
    public float xSpacing;
    public float zSpacing;
    public string[] geoSystem;
    public GameObject geoOrigin;
    public GameObject color;
    public bool colorPerVertex = true;
    public GameObject normal;
    public bool normalPerVertex = true;
    public GameObject texCoord;
    public bool ccw = true;
    public bool solid = true;
    public float creaseAngle = 0.0f;
}
}
