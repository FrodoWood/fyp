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
    public Material purpleMaterial;
    public Material blueMaterial;
    private Renderer _renderer;

    private void Start()
    {
        initialPosition = transform.position;
        distanceTravelled = 0.01f;
    }
    private void Update()
    {
        distanceTravelled = Vector3.Distance(initialPosition, transform.position);
        if(distanceTravelled > 30f)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(float damageValue, EntityType entityType, EnemyController enemyController)
    {
        damage = damageValue;
        entity = entityType;
        agentController = enemyController;

        _renderer = GetComponent<Renderer>();
        if (entityType == EntityType.Enemy) _renderer.material = purpleMaterial;
        else _renderer.material = blueMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();

        if (damageable != null && damageable.GetEntityType() != entity)
        {
            if(distanceTravelled < 5f)
            {
                agentController?.AddReward(-1f + distanceTravelled/5);
                agentController?.AddScore(5);
            }
            else
            {
                agentController?.AddReward(4 * distanceTravelled/30f);
                agentController?.AddScore(10);
            }
            //Debug.Log("Enemy Hit!");
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }

        if (other.gameObject.CompareTag("wall"))
        {
            agentController?.AddReward(-0.1f);
            Destroy(gameObject);
        }
    }
}
