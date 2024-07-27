using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    // Declare Variables for Health and Game Object
    private GameObject EntityObj;
    public int entityHealth;
    private int currentHealth;
    public string opposingEntity;

    // Start is called before the first frame update
    void Start()
    {
        EntityObj = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Hitbox Functions (Collider)
    void OnTriggerEnter(Collider other)
    {
       if (other.gameObject.tag == opposingEntity)
        {
            Stats h = other.gameObject.GetComponent<Stats>();
            h.entityHealth -= 20;
            print("Enter");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == opposingEntity)
        {

            print("Stay");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == opposingEntity)
        {
            print("Exit");
        }
    }

    // Function to check if entity is alive or dead
    public bool IsAlive()
    {
        if (entityHealth >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Function For Damage Done to Enemy
    void damageDone(int damage)
    {
        entityHealth -= damage;
    }

}