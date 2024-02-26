using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

public class EnemyController : Agent
{
    public float moveSpeed = 6f;
    private CharacterController controller;
    private BufferSensorComponent bufferSensor;
    Vector3 mousePos;
    Vector3 movement;
    public Transform player;
    public Transform ball;
    public Transform gun;
    public Transform objectToLookAt;
    public float health = 50f;
    private Bullet[] bullets;

    [Header("Gizmos")]
    public float bulletGizmoRadius = 1f;


    public override void Initialize()
    {
        controller = GetComponent<CharacterController>();
        bufferSensor = GetComponent<BufferSensorComponent>();
        bullets = FindObjectsOfType<Bullet>();
        randomizePosition();
    }

    public override void OnEpisodeBegin()
    {
        movement = Vector3.zero;

        // move target (Player) to a new random position
        //player.localPosition = new Vector3(Random.value * 20 - 10, 0f, Random.value * 20 - 10);
        randomizePosition();
        health = 50f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // agent position
        //sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localPosition.x/15f);
        sensor.AddObservation(transform.localPosition.z/15f);
        //Debug.Log(transform.localPosition.x / 15f);

        // agent velocity
        //sensor.AddObservation(controller.velocity.x);
        //sensor.AddObservation(controller.velocity.z);
        // target (player) position
        //sensor.AddObservation(player.localPosition);


        // Variable length observations
        bullets = FindObjectsOfType<Bullet>()
            .Where(bullet => bullet.transform.parent == transform.parent)
            .OrderBy(bullet => Vector3.Distance(transform.localPosition, bullet.transform.localPosition))
            .Take(5)
            .ToArray();

        foreach (Bullet bullet in bullets)
        {
            float[] bulletObservation = new float[]
            {
                (bullet.transform.localPosition.x - transform.localPosition.x) / 15f,
                (bullet.transform.localPosition.z - transform.localPosition.z) / 15f,
                (bullet.transform.localPosition.x) / 15f,
                (bullet.transform.localPosition.z) / 15f,
                bullet.transform.forward.x,
                bullet.transform.forward.z
            };
            //Debug.Log("\n" + bulletObservation[0] + bulletObservation[1]);
            bufferSensor.AppendObservation(bulletObservation);
        }
        //Debug.Log("Closest bullet's position" + " x:" + bullets[0].transform.localPosition.x + "" + " z" + bullets[0].transform.localPosition.z);
        //Debug.Log("Closest bullet's forward" + " x:" + bullets[0].transform.forward.x + "" + " z" + bullets[0].transform.forward.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        controller.Move(controlSignal * Time.deltaTime * moveSpeed);

        // Rewards
        //if (health <= 0)
        //{
        //    //AddReward(-0.1f);
        //    EndEpisode();
        //}

        //if(StepCount < 500) AddReward(0.001f);

        //switch (StepCount)
        //{
        //    case 500:
        //        AddReward(0.1f);
        //        break;
        //    case 750:
        //        AddReward(0.15f);
        //        break;
        //    case 1000:
        //        AddReward(0.25f);
        //        Debug.Log("Perfect!!");
        //        break;

        //}


        if (this.StepCount % 100 == 0)
        {
            AddReward(0.1f);
        }
    }

    private void Update()
    {
        transform.LookAt(objectToLookAt);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("wall"))
        {
            //AddReward(-(currentReward / 2));

            //AddReward(-0.5f);
            AddReward(-0.5f);
            float currentReward = GetCumulativeReward();
            if (currentReward < 0f)
            {
                SetReward(0f);
            }
            EndEpisode();
            Debug.Log("DEAD by wall!");
        }
        if (other.CompareTag("bullet"))
        {
            //AddReward(-0.1f);
            //AddReward(-(currentReward / 3));
            //AddReward(-1);
            //float currentReward = GetCumulativeReward();
            //if (currentReward < 0f)
            //{
            //    SetReward(0f);
            //}
            //EndEpisode();
            SetReward(0f);
            EndEpisode();
            //health -= 10f;
            Debug.Log("DEAD by bullet!");
        }
    }

    void randomizePosition()
    {
        Vector3 newPos = new Vector3((Random.value * 28) - 14f, 0f, (Random.value * 28) - 14f);
        controller.Move(newPos - transform.localPosition);
        //this.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    private void OnDrawGizmos()
    {
        if(bullets != null && bullets.Length > 0)
        {
            foreach(Bullet bullet in bullets)
            {
                if(bullet == null) continue;
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(bullet.transform.position, bulletGizmoRadius);
            }
        }
    }

}
