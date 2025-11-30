using UnityEngine;

namespace X3D {
public class X3DMovieTextureComponent : MonoBehaviour {
    public string url;
    public bool loop = false;
    public float speed = 1.0f;
    public float startTime = 0f;
    public float stopTime = 0f;
    public bool repeatS = true;
    public bool repeatT = true;
    public string[] urls;
    public string metadata;
}
}
