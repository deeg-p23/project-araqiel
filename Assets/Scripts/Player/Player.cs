using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Reference Health Bar
    public HealthValues healthBar;
    public Stats healthStats;

    // Declare Health Variables
    public int currentHealth;
    

    // Load in Current Health
    void Start()
    {
        int maxHealth = healthStats.entityHealth;
        currentHealth = maxHealth;
        healthBar.setMaxHealth(currentHealth);
    }

    // Damage test
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            damageTaken(20);
            Debug.Log(currentHealth);
        }
    }

    // Function for damage taken
    void damageTaken(int damage)
    {
        currentHealth -= damage;
        healthBar.setHealth(currentHealth);
    }


}
