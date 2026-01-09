using UnityEngine;

namespace X3D
{
    // X3D TouchSensor event handling
    public class X3DTouchSensor : MonoBehaviour
    {
        // X3D optional fields
    public bool x3dEnabled = true;
    public string description;
    public string metadata;
    public string DEF;
    public string USE;
    public string containerField;

        void OnMouseDown()
        {
            if (x3dEnabled)
                Debug.Log($"X3D TouchSensor activated: {description}");
        }
    }
}