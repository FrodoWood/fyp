using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public EntityType entity { get; private set; }
    private EnemyController agentController;
    private float distanceTravelled;
    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
        distanceTravelled = 0.01f;
    }
    private void Update()
    {
        distanceTravelled = Vector3.Distance(initialPosition, transform.position);
        if(distanceTravelled > 20f)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(float damageValue, EntityType entityType, EnemyController enemyController)
    {
        damage = damageValue;
        entity = entityType;
        agentController = enemyController;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();

        if (damageable != null && damageable.GetEntityType() != entity)
        {
            if(distanceTravelled < 5f)
            {
                agentController?.AddReward(1.5f - distanceTravelled/5);
            }
            else
            {
                agentController?.AddReward(2 * distanceTravelled/20f);
            }
            //Debug.Log("Enemy Hit!");
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }

        if (other.gameObject.CompareTag("wall"))
        {
            Destroy(gameObject);
        }
    }
}
