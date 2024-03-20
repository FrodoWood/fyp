using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public EntityType entity { get; private set; }
    private EnemyController agentController;

    public void Initialize(float damageValue, EntityType entityType, EnemyController enemyController)
    {
        damage = damageValue;
        entity = entityType;
        agentController = enemyController;
    }

    private void OnCollisionEnter(Collision collision)
    {
        

    }
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();

        if (damageable != null && damageable.GetEntityType() != entity)
        {
            agentController?.AddReward(damage/agentController.maxHealth);
            Debug.Log("Enemy Hit!");
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }

        if (other.gameObject.CompareTag("wall"))
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        //Destroy(gameObject, 4f);
    }
}
