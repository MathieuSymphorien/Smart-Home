using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensorLinkUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Dropdown sensorDD;
    [SerializeField] TMP_Dropdown lightDD;
    [SerializeField] Button addBtn, removeBtn;
    [SerializeField] Slider rangeSlider, fovSlider;

    /* runtime */
    List<MotionSensor> sensors = new();
    List<Light>        lights  = new();
    MotionSensor       highlighted;          // sensor actuellement “surligné”

    void Awake()          // Awake plutôt que Start pour être prêt plus tôt
    {
        RefreshLists();

        sensorDD.onValueChanged.AddListener(OnSensorChanged);
        addBtn.onClick   .AddListener(LinkSelected);
        removeBtn.onClick.AddListener(UnlinkSelected);

        rangeSlider.onValueChanged.AddListener(v => CurrentSensor()?.SetRange(v));
        fovSlider  .onValueChanged.AddListener(v => CurrentSensor()?.SetFov(v));

        MotionSensor.OnSettingsChanged += _ => RefreshSensorValues();
    }

    /* ------------- LISTES ------------- */
    public void RefreshLists()
{
    sensors = FindObjectsOfType<MotionSensor>()
              .OrderBy(s => s.name).ToList();

    lights  = FindObjectsOfType<Light>()
              .Where(l => l.type != LightType.Directional)   // ⬅️ filtre !
              .OrderBy(l => l.name).ToList();

    sensorDD.ClearOptions();
    sensorDD.AddOptions(sensors.Select(s => s.name).ToList());

    lightDD.ClearOptions();
    lightDD.AddOptions(lights .Select(l => l.name).ToList());

    OnSensorChanged(sensorDD.value);
}


    /* ------------- VALEURS ------------- */
    void RefreshSensorValues()
    {
        var s = CurrentSensor();
        if (!s) return;

        rangeSlider.SetValueWithoutNotify(s.range);
        fovSlider  .SetValueWithoutNotify(s.fov);
    }

    /* ------------- HELPERS ------------- */
    MotionSensor CurrentSensor() =>
        sensorDD.value >= 0 && sensorDD.value < sensors.Count ? sensors[sensorDD.value] : null;

    Light CurrentLight() =>
        lightDD.value >= 0 && lightDD.value < lights.Count ? lights[lightDD.value] : null;

    /* ------------- ACTIONS ------------- */
    void LinkSelected   () { CurrentSensor()?.LinkLight(CurrentLight());   }
    void UnlinkSelected () { CurrentSensor()?.UnlinkLight(CurrentLight()); }

    /* ------------- CHANGEMENT SENSOR ------------- */
    void OnSensorChanged(int _)
    {
        if (highlighted) highlighted.SetHighlight(false);   // éteint l’ancien

        highlighted = CurrentSensor();
        if (highlighted) highlighted.SetHighlight(true);    // allume le nouveau

        RefreshSensorValues();
    }
}
