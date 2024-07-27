using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
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
        healthBar.setMaxHealth(maxHealth);
    }

    // Damage test
    void Update()
    {
        if(currentHealth != healthStats.entityHealth)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentHealth = healthStats.entityHealth;
                healthBar.setHealth(currentHealth);
            }
        }
    }

    // Function for damage taken
    void damageTaken(int damage)
    {
        currentHealth -= damage;
        healthBar.setHealth(currentHealth);
    }
}
