using UnityEngine;

// Stub for X3D TimeSensor node
namespace X3D {
public class X3DTimeSensor : MonoBehaviour
{
    public float cycleInterval = 1.0f;
    public bool loop = false;
    public float startTime = 0f;
    public float stopTime = 0f;
    public float fraction = 0f;
    public bool enabledSensor = true;
    public bool isActive = false;
    public float elapsed = 0f;
    public string metadata;

    public delegate void TimeSensorEvent(float fraction);
    public event TimeSensorEvent OnFractionChanged;
    public event System.Action OnCycleTime;
    public event System.Action OnIsActive;

    void Start()
    {
        if (enabledSensor && startTime == 0f) StartSensor();
    }

    void Update()
    {
        if (!enabledSensor || !isActive) return;
        elapsed += Time.deltaTime;
        float localTime = elapsed;
        if (cycleInterval > 0f)
        {
            fraction = Mathf.Clamp01((localTime % cycleInterval) / cycleInterval);
            OnFractionChanged?.Invoke(fraction);
            if (localTime >= cycleInterval)
            {
                OnCycleTime?.Invoke();
                if (loop)
                {
                    elapsed = 0f;
                }
                else
                {
                    StopSensor();
                }
            }
        }
        if (stopTime > 0f && elapsed >= stopTime)
        {
            StopSensor();
        }
    }

    public void StartSensor()
    {
        isActive = true;
        elapsed = 0f;
        OnIsActive?.Invoke();
    }

    public void StopSensor()
    {
        isActive = false;
    }
}
}
