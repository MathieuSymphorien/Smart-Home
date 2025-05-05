
using UnityEngine;

public class LightController : MonoBehaviour
{
    Light _l;
    int   activeSensors = 0;        // nombre de capteurs qui “voient”

    void Awake() => _l = GetComponent<Light>();

    public void SensorState(bool seen)
    {
        if (seen)  ++activeSensors;
        else       --activeSensors;
        activeSensors = Mathf.Max(0, activeSensors);

        _l.enabled = activeSensors > 0;
    }
}
