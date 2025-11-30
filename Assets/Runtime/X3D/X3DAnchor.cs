using UnityEngine;

namespace X3D
{
    // X3D Anchor event handling
    public class X3DAnchor : MonoBehaviour
    {
    public string url;
    public string[] parameter;
    public string description;
    public string metadata;
    public string DEF;
    public string USE;
    public string containerField;
        void OnMouseDown()
        {
            Debug.Log($"X3D Anchor activated: {url}");
        }
    }
}