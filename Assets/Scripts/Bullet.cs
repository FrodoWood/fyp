using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damageAmount = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if(damageable != null)
        {
            damageable.OnHit(damageAmount);
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        Destroy(gameObject,5f);
    }
}
