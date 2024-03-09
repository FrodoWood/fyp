using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using UnityEngine.AI;

public class EnemyController : Agent
{
    private NavMeshAgent navMeshAgent;
    [SerializeField] private float actionCooldown;
    private float actionTimer = 0f;


    [Header("Gizmos")]
    public float bulletGizmoRadius = 1f;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        actionTimer += Time.deltaTime;
    }


    public override void Initialize()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(navMeshAgent.transform.position / 15f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if(actionTimer >= actionCooldown)
        {
            float xDestination = actions.ContinuousActions[0] * 15f;
            float zDestination = actions.ContinuousActions[1] * 15f;

            navMeshAgent.SetDestination(new Vector3(xDestination, 0f, zDestination));
            Debug.Log("Model chose destination");
            actionTimer = 0f;
        }

        AddReward(1f/MaxStep);
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
            SetReward(0f);
            EndEpisode();
            Debug.Log("DEAD by wall!");
        }
        if (other.CompareTag("bullet"))
        {
            SetReward(0f);
            EndEpisode();
            Debug.Log("DEAD by bullet!");
        }
    }

    void randomizePosition()
    {
        
    }

    private void OnDrawGizmos()
    {
        
    }

}
