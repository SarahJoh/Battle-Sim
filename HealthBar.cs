using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public static HealthBar Instance { set; get; }

    public Slider slider;

    public void Start()
    {
        Instance = this;
    }

    /** Sets the slider to its maximum value. */
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    /** Adjusts the sliders value. */
    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
