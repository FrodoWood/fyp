using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour, IDamageable 
{
    public float health = 50f;
    public static event Action<float> onEnemyHit;
    public static event Action onEnemyDeath;
    
    public void OnHit(float damageAmount)
    {
        onEnemyHit?.Invoke(damageAmount);
        health -= damageAmount;

        if (health <= 0)
        {
            onEnemyDeath?.Invoke();
        }
    }

    public bool isAlive()
    {
        return health > 0;
    }

    public void resetHealth()
    {
        health = 50;
    }
}
