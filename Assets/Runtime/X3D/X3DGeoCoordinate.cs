using UnityEngine;

namespace X3D {
public class X3DGeoCoordinate : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // GeoCoordinate fields
    public double[] point;
    public string[] geoSystem;
    public string geoCoords;
}
}
