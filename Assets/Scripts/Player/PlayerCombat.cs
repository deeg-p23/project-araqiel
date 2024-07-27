using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Reference Animator for Attack Animations
    public Animator animator;
    
    // Create a list for Attacks 
    public List<AttackInfo> attackList = new List<AttackInfo>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            Attack();
        }

    }

    // Create Attack Function
    void Attack()
    {

        animator.SetTrigger("Attack");
    }
    
    // Function to load in AttackInfo
    void addtoAttackList(AttackInfo info)
    {
        attackList.Add(info);
    }
    
}
