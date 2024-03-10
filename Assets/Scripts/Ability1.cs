using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability1 : BaseAbility
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 25f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;

    public void Initialize(EntityType entityType)
    {
        entity = entityType;
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    [ContextMenu("useAbility1")]
    public override void TriggerAbility()
    {
        base.TriggerAbility(); // Starts the cooldown timer and sets the ability on cooldown
        GameObject newBullet = GameObject.Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bullet = newBullet.GetComponent<Bullet>();
        bullet?.Initialize(damage, entity, enemyController);

        newBullet.transform.parent = transform.parent;
        Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
        Vector3 direction = transform.forward;
        newBullet.transform.rotation = Quaternion.LookRotation(direction);
        bulletRB.AddForce(direction.normalized * speed, ForceMode.VelocityChange);

        GameObject.Destroy(newBullet, 2f);

    }
}
