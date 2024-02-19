using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable 
{
    public float health = 50f;
    public void OnHit(float damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            //Destroy(gameObject);
        }
    }

}
