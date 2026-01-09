using UnityEngine;

namespace X3D {
public class X3DNavigationInfo : MonoBehaviour
{
    // Universal fields
    public string DEF;
    public string USE;
    public string containerField;
    public string metadata;

    // NavigationInfo fields
    public string[] type;
    public float[] avatarSize;
    public bool headlight = true;
    public float speed = 1.0f;
    public float visibilityLimit = 0.0f;
    public string[] transitionType;
    public float transitionTime = 1.0f;
}
}
