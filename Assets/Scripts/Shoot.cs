using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public Transform firingPoint;
    public GameObject bullet;
    public float bulletSpeed = 10f;
    private PlayerController playerController;
    private bool canShoot = true;
    private float shootCooldown = 1f;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.getIsPlayerControlled())
        {
            if (Input.GetMouseButtonDown(0))
            {
                shootBullet();
            }
        }
        else
        {
            // Player is AI controlled and should fire a bullet every 0.5 seconds
            if (canShoot)
            {
                StartCoroutine(fireBulletRoutine());
                canShoot = false;
            }
        }
        
    }

    private void shootBullet()
    {
        GameObject newBullet = Instantiate(bullet, firingPoint.position, firingPoint.rotation);
        Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
        bulletRB.AddForce(firingPoint.forward * bulletSpeed, ForceMode.VelocityChange);
    }

    IEnumerator fireBulletRoutine()
    {
        yield return new WaitForSeconds(shootCooldown);
        shootBullet();
        canShoot = true;
    }
}
