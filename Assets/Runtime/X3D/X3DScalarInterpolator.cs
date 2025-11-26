using UnityEngine;

// Stub for X3D ScalarInterpolator node
namespace X3D {
public class X3DScalarInterpolatorStub : MonoBehaviour
{
    public float[] keyValues;
    public float[] keys;
    public float fraction;
    public delegate void ValueChanged(float value);
    public event ValueChanged OnValueChanged;
    public string metadata;

    void Update()
    {
        float value = GetInterpolatedValue(fraction);
        OnValueChanged?.Invoke(value);
    }

    public float GetInterpolatedValue(float t)
    {
        if (keys == null || keyValues == null || keys.Length == 0 || keyValues.Length == 0)
            return 0f;
        if (t <= keys[0]) return keyValues[0];
        if (t >= keys[keys.Length - 1]) return keyValues[keyValues.Length - 1];
        for (int i = 0; i < keys.Length - 1; i++)
        {
            if (t >= keys[i] && t <= keys[i + 1])
            {
                float localT = (t - keys[i]) / (keys[i + 1] - keys[i]);
                return Mathf.Lerp(keyValues[i], keyValues[i + 1], localT);
            }
        }
        return 0f;
    }
}
}
