using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public Transform firingPoint;
    public GameObject bullet;
    public float bulletSpeed = 10f;
    private PlayerController playerController;
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
                GameObject newBullet =  Instantiate(bullet, firingPoint.position, firingPoint.rotation);
                Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
                bulletRB.AddForce(firingPoint.forward * bulletSpeed, ForceMode.VelocityChange);
            }
        }
        else
        {

        }
    }
}
