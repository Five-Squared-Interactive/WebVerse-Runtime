using UnityEngine;

// X3D OrientationInterpolator node
namespace X3D {
public class X3DOrientationInterpolator : MonoBehaviour
{
    public Quaternion[] keyValues;
    public float[] keys;
    public float fraction;
    public delegate void ValueChanged(Quaternion value);
    public event ValueChanged OnValueChanged;
    public string metadata;

    void Update()
    {
        Quaternion value = GetInterpolatedValue(fraction);
        OnValueChanged?.Invoke(value);
    }

    public Quaternion GetInterpolatedValue(float t)
    {
        if (keys == null || keyValues == null || keys.Length == 0 || keyValues.Length == 0)
            return Quaternion.identity;
        if (t <= keys[0]) return keyValues[0];
        if (t >= keys[keys.Length - 1]) return keyValues[keyValues.Length - 1];
        for (int i = 0; i < keys.Length - 1; i++)
        {
            if (t >= keys[i] && t <= keys[i + 1])
            {
                float localT = (t - keys[i]) / (keys[i + 1] - keys[i]);
                return Quaternion.Slerp(keyValues[i], keyValues[i + 1], localT);
            }
        }
        return Quaternion.identity;
    }
}
}
