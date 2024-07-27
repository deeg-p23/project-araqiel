using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthValues : MonoBehaviour
{
    // Create Function for Health Bar Value
    public Slider slider;

    // Create Functions For Setting Health Values

    public void setMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }
    public void setHealth(int health)
    {
        slider.value = health;
    }
}
