using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    private EntityType entity;
    private EnemyController agentController;

    public void Initialize(float damageValue, EntityType entityType, EnemyController enemyController)
    {
        damage = damageValue;
        entity = entityType;
        agentController = enemyController;
    }

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if(damageable != null && damageable.GetEntityType() != entity)
        {
            agentController?.AddReward(1f);
            Debug.Log("Enemy Hit!");
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
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
