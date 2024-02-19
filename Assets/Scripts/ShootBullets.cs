using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShootBullets : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireCooldown = 0.5f;
    private bool canFire = true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canFire)
        {
            StartCoroutine(fireBullets());
            canFire = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            shoot();
        }
    }

    private IEnumerator fireBullets()
    {
        yield return new WaitForSeconds(fireCooldown);
        shoot();
        canFire = true;
    }

    private void shoot()
    {
        float randomX = (Random.value - 0.5f) * 2;
        float randomZ = (Random.value - 0.5f) * 2;

        GameObject newBullet = Instantiate(bulletPrefab, new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ ), Quaternion.identity);
        Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
        Vector3 direction =  newBullet.transform.position - transform.position;
        bulletRB.AddForce(direction.normalized * 5, ForceMode.VelocityChange);
    }
}
