using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    private CharacterController controller;
    private Vector3 mousePos;
    private Vector3 targetPosition;
    public Transform enemy;

    [SerializeField] private bool isPlayerControlled = true;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        setRandomTargetPosition();
    }

    void Update()
    {
        if (isPlayerControlled)
        {
            playerControls();
        }
        else
        {
            aiControls();
        }
    }

    void playerControls()
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

            // Rotate towards the mouse position
            Vector3 lookAtPos = new Vector3(point.x, transform.position.y, point.z);
            transform.LookAt(lookAtPos);
        }
    }

    void aiControls()
    {
        // Move towards the random target
        Vector3 movementDirection = (targetPosition - transform.position).normalized;
        controller.Move(movementDirection * Time.deltaTime * moveSpeed);

        // Look towards the enemy
        transform.LookAt(enemy.transform.position);

        // Check if close the target, if yes then set new random target
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            setRandomTargetPosition();
        }

        // Avoid the enemy
        if(Vector3.Distance(transform.position, enemy.transform.position) < 5f)
        {
            Vector3 enemyOppositeDirection = (transform.position - enemy.transform.position).normalized;
            controller.Move(enemyOppositeDirection * Time.deltaTime * moveSpeed);
        }

    }

    void setRandomTargetPosition()
    {
        float x = Random.Range(-15f, 15f);
        float z = Random.Range(-15f, 15f);
        targetPosition = new Vector3(x, 0f, z);
    }

    public bool getIsPlayerControlled()
    {
        return isPlayerControlled;
    }
}
