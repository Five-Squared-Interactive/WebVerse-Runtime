using UnityEngine;

namespace X3D {
public class X3DMaterialComponent : MonoBehaviour {
    public Color diffuseColor = Color.white;
    public Color emissiveColor = Color.black;
    public Color specularColor = Color.black;
    public Color ambientIntensityColor = Color.black;
    public float ambientIntensity = 0.2f;
    public float shininess = 0.2f;
    public float transparency = 0f;
    public bool isSmooth = true;
    public bool isLit = true;
    public string metadata;
}
}
