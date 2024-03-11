using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyTarget : MonoBehaviour, IDamageable
{
    public float maxHealth = 20f;
    public float currentHealth;
    public EnemyController enemyController;

    private void Start()
    {
        currentHealth = maxHealth;
    }
    public EntityType GetEntityType()
    {
        return EntityType.Player;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if(currentHealth <= 0)
        {
            enemyController?.EndEpisode();  
        }
    }
}
