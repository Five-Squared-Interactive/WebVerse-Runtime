using UnityEngine;

namespace X3D
{
    // Placeholder for X3D Script node data
    public class X3DScriptComponent : MonoBehaviour
    {
        [TextArea]
        public string scriptSource;
        public string[] urls;
        public string metadata;
    }
}
