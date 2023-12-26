using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class EnemyController : Agent
{
    public float moveSpeed = 6f;
    private CharacterController controller;
    Vector3 mousePos;
    Vector3 movement;
    public Transform player;
    public Transform ball;
    public Transform gun;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        randomizePosition();
    }

    public override void OnEpisodeBegin()
    {
        movement = Vector3.zero;
        //this.transform.localPosition = new Vector3(13,0,0);
        //this.transform.localPosition = new Vector3(Random.value * 28 - 14, 0f, Random.value * 28 - 14);
        //this.transform.localRotation = Quaternion.Euler(Vector3.zero);

        // move target (Player) to a new random position
        player.localPosition = new Vector3(Random.value * 20 - 10, 0f, Random.value * 20 - 10);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // agent position
        sensor.AddObservation(transform.localPosition);
        // agent velocity
        sensor.AddObservation(controller.velocity.x);
        sensor.AddObservation(controller.velocity.z);
        // target (player) position
        sensor.AddObservation(player.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        controller.Move(controlSignal * Time.deltaTime * moveSpeed);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, player.localPosition);
        if(distanceToTarget < 1.5f)
        {
            SetReward(1f);
            EndEpisode();
        }
        else if(this.transform.localPosition.x > 15f || this.transform.localPosition.x < -15f || this.transform.localPosition.z > 15f || this.transform.localPosition.z < -15f)
        {
            SetReward(-0.5f);
            randomizePosition();
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    void Update()
    {
        //float horizontal = Input.GetAxis("Horizontal");
        //float vertical = Input.GetAxis("Vertical");

        //movement = new Vector3(horizontal, 0, vertical);
        //controller.Move(movement * Time.deltaTime * moveSpeed);


        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Plane groundPlane = new Plane(Vector3.up, transform.position);

        //float rayDistance;
        //if (groundPlane.Raycast(ray, out rayDistance))
        //{
        //    Vector3 point = ray.GetPoint(rayDistance);
        //    mousePos = new Vector3(point.x, transform.position.y, point.z);

        //    // Rotate towards the mouse position
        //    Vector3 lookAtPos = new Vector3(point.x, transform.position.y, point.z);
        //    transform.LookAt(lookAtPos);
        //}
    }

    void randomizePosition()
    {
        this.transform.localPosition = new Vector3(Random.value * 20 - 10, 0f, Random.value * 20 - 10);
        //this.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

}
