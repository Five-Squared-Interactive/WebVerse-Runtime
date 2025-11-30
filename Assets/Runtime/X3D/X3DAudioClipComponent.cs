using UnityEngine;

namespace X3D {
public class X3DAudioClipComponent : MonoBehaviour {
    public string url;
    public string description;
    public bool loop = false;
    public float pitch = 1.0f;
    public float startTime = 0f;
    public float stopTime = 0f;
    public string[] urls;
    public string metadata;
}
}
