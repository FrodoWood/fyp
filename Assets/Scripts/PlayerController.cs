using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    private CharacterController controller;
    Vector3 mousePos;
    public Transform ball;
    public Transform gun;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical);
        controller.Move(movement * Time.deltaTime * moveSpeed);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            mousePos = new Vector3(point.x, transform.position.y, point.z);
            ball.position = mousePos;

            // Rotate towards the mouse position
            Vector3 lookAtPos = new Vector3(point.x, transform.position.y, point.z);
            transform.LookAt(lookAtPos);
            //gun.transform.LookAt(lookAtPos);
            //gun.transform.rotation = Quaternion.Euler(0f, gun.transform.rotation.eulerAngles.y, 0f);
            
        }
    }
}
