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
    public Transform objectToLookAt;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        lookAtTarget();
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

    private void lookAtTarget()
    {
        transform.LookAt(objectToLookAt.position);
        transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f) + new Vector3(0f, (Random.value * 50) - 25f, 0f);

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

        //GameObject newBullet = Instantiate(bulletPrefab, new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ ), Quaternion.identity);
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        newBullet.transform.parent = transform.parent;
        Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
        Vector3 direction = transform.forward;
        bulletRB.AddForce(direction.normalized * 5, ForceMode.VelocityChange);
    }
}
