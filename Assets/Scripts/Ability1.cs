using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability1 : BaseAbility
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 25f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;

   
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

        // Instantiate bullet prefab, initialize its damage and entity and inject the enemyController reference
        GameObject newBullet = GameObject.Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bullet = newBullet.GetComponent<Bullet>();
        bullet?.Initialize(damage, entity, enemyController);

        newBullet.transform.parent = transform.parent; // Set the environemnt as the bullet's parent so it stays within the env prefab
        Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
        Vector3 direction = transform.forward;
        newBullet.transform.rotation = Quaternion.LookRotation(direction);

        // Add a force to the bullet prefab in the forward direction in which it was instantiated/fired by the agent
        bulletRB.AddForce(direction.normalized * speed, ForceMode.VelocityChange);

    }

}
