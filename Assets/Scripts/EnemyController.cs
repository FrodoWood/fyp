using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using UnityEngine.AI;
using System;

public enum State
{
    Idle,
    Moving,
    Stunned,
    Ability1,
    Ability2,
    Ability3,
    Ability4,
    Dead,
}

public enum EntityType
{
    Enemy,
    Player,
}

public class EnemyController : Agent, IDamageable
{
    [SerializeField] private EntityType entity;
    private NavMeshAgent navMeshAgent;
    [SerializeField] private float actionCooldown;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isStunned;
    [SerializeField] private LayerMask movementLayers;
    private float actionTimer = 0f;
    private State currentState;

    [Header("Gizmos")]
    public float bulletGizmoRadius = 1f;

    const int a_idle = 0;
    const int a_moving = 1;
    const int a_ability1 = 2;
    const int a_ability2 = 3;
    const int a_ability3 = 4;
    const int a_ability4 = 5;

    private ActionBuffers actionBuffers;

    private Ability1 ability1;
    private Ability2 ability2;
    private Ability3 ability3;
    private Ability4 ability4;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
        ability1 = GetComponent<Ability1>();
        ability2 = GetComponent<Ability2>();
        ability3 = GetComponent<Ability3>();
        ability4 = GetComponent<Ability4>();
    }

    private void Start()
    {
        ChangeState(State.Idle);
    }
    private void Update()
    {
        actionTimer += Time.deltaTime;
        UpdateCurrentState();
    }


    public override void Initialize()
    {
        ChangeState(State.Idle);
        isStunned = false;
    }

    public override void OnEpisodeBegin()
    {
        ChangeState(State.Idle);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(navMeshAgent.transform.position / 15f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if(actionTimer >= actionCooldown)
        {
            actionBuffers = actions;
            UpdateStateOnActionReceived();

            actionTimer = 0f;
        }
        
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Setting continuous actions
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, movementLayers);
        Vector3 movePosition = hit.point.normalized;
        continuousActionsOut[0] = movePosition.x;
        continuousActionsOut[1] = movePosition.z;

        // Setting discrete actions
        if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = a_idle;
        if (Input.GetKey(KeyCode.Q)) discreteActionsOut[0] = a_ability1;
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = a_ability2;
        if (Input.GetKey(KeyCode.E)) discreteActionsOut[0] = a_ability3;
        if (Input.GetKey(KeyCode.R)) discreteActionsOut[0] = a_ability4;
        else discreteActionsOut[0] = a_moving;
    }

    private void ChangeState(State newState)
    {
        ExitCurrentState();
        currentState = newState;
        EnterCurrentState();
    }

    private void EnterCurrentState()
    {
        switch (currentState)
        {
            case State.Idle:
                EnterIdle();
                break;
            case State.Moving:
                EnterMoving();
                break;
            case State.Stunned:
                EnterStunned();
                break;
            case State.Ability1:
                EnterAbility1();
                break;
            case State.Ability2:
                EnterAbility2();
                break;
            case State.Ability3:
                EnterAbility3();
                break;
            case State.Ability4:
                EnterAbility4();
                break;
            case State.Dead:
                EnterDead();
                break;
        }
    }
    private void UpdateCurrentState()
    {
        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Moving:
                UpdateMoving();
                break;
            case State.Stunned:
                UpdateStunned();
                break;
            case State.Ability1:
                UpdateAbility1();
                break;
            case State.Ability2:
                UpdateAbility2();
                break;
            case State.Ability3:
                UpdateAbility3();
                break;
            case State.Ability4:
                UpdateAbility4();
                break;
            case State.Dead:
                UpdateDead();
                break;
        }
    }
    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case State.Idle:
                ExitIdle();
                break;
            case State.Moving:
                ExitMoving();
                break;
            case State.Stunned:
                ExitStunned();
                break;
            case State.Ability1:
                ExitAbility1();
                break;
            case State.Ability2:
                ExitAbility2();
                break;
            case State.Ability3:
                ExitAbility3();
                break;
            case State.Ability4:
                ExitAbility4();
                break;
            case State.Dead:
                ExitDead();
                break;
        }
    }
    private void UpdateStateOnActionReceived()
    {
        switch (currentState)
        {
            case State.Idle:
                UpdateIdleOnActionReceived();
                break;
            case State.Moving:
                UpdateMovingOnActionReceived();
                break;
            case State.Stunned:
                UpdateStunnedOnActionReceived();
                break;
            case State.Ability1:
                UpdateAbility1OnActionReceived();
                break;
            case State.Ability2:
                UpdateAbility2OnActionReceived();
                break;
            case State.Ability3:
                UpdateAbility3OnActionReceived();
                break;
            case State.Ability4:
                UpdateAbility4OnActionReceived();
                break;
            case State.Dead:
                UpdateDeadOnActionReceived();
                break;
        }
    }


    private void EnterIdle()
    {
        navMeshAgent.isStopped = true;
    }
    private void UpdateIdle()
    {
        
    }
    private void ExitIdle()
    {
        
    }
    private void UpdateIdleOnActionReceived()
    {
        switch (actionBuffers.DiscreteActions[0])
        {
            case a_idle:

                break;
            case a_moving:
                ChangeState(State.Moving);
                break;
            case a_ability1:
                ChangeState(State.Ability1);
                break;
            case a_ability2:
                ChangeState(State.Ability2);
                break;
            case a_ability3:
                ChangeState(State.Ability3);
                break;
            case a_ability4:
                ChangeState(State.Ability4);
                break;

        }
    }

    private void EnterMoving()
    {
        navMeshAgent.isStopped = false;

    }
    private void UpdateMoving()
    {

    }
    private void ExitMoving()
    {
        
    }
    private void UpdateMovingOnActionReceived()
    {
        float xDestination = actionBuffers.ContinuousActions[0] * 15f;
        float zDestination = actionBuffers.ContinuousActions[1] * 15f;
        navMeshAgent.SetDestination(new Vector3(xDestination, 0f, zDestination));
        Debug.Log("Model chose destination");

        switch (actionBuffers.DiscreteActions[0])
        {
            case a_idle:
                ChangeState(State.Idle);
                break;

            case a_moving:
                break;

            case a_ability1:
                if(ability1.Available()) ChangeState(State.Ability1);
                break;

            case a_ability2:
                if (ability2.Available()) ChangeState(State.Ability1);
                ChangeState(State.Ability2);
                break;

            case a_ability3:
                if (ability3.Available()) ChangeState(State.Ability1);
                ChangeState(State.Ability3);
                break;

            case a_ability4:
                if (ability4.Available()) ChangeState(State.Ability1);
                ChangeState(State.Ability4);
                break;
        }
    }

    private void EnterAbility1()
    {
        Debug.Log("Entered ability1");
        navMeshAgent.isStopped = true;
        ability1.TriggerAbility();
    }
    private void UpdateAbility1()
    {
        
    }
    private void ExitAbility1()
    {
        
    }
    private void UpdateAbility1OnActionReceived()
    {
        switch (actionBuffers.DiscreteActions[0])
        {
            case a_idle:
                ChangeState(State.Idle);
                break;

            case a_moving:
                ChangeState(State.Moving);
                break;
            
            default:
                ChangeState(State.Moving);
                break;
        }
    }

    private void EnterAbility2()
    {
        Debug.Log("Entered ability2");
        navMeshAgent.isStopped = true;
        ability2.TriggerAbility();
    }
    private void UpdateAbility2()
    {
        
    }
    private void ExitAbility2()
    {
        
    }
    private void UpdateAbility2OnActionReceived()
    {
        switch (actionBuffers.DiscreteActions[0])
        {
            case a_idle:
                ChangeState(State.Idle);
                break;

            case a_moving:
                ChangeState(State.Moving);
                break;

            default:
                ChangeState(State.Moving);
                break;
        }
    }

    private void EnterAbility3()
    {
        Debug.Log("Entered ability3");
        ability3.TriggerAbility();
    }
    private void UpdateAbility3()
    {
        
    }
    private void ExitAbility3()
    {
        
    }
    private void UpdateAbility3OnActionReceived()
    {
        switch (actionBuffers.DiscreteActions[0])
        {
            case a_idle:
                ChangeState(State.Idle);
                break;

            case a_moving:
                ChangeState(State.Moving);
                break;

            default:
                ChangeState(State.Moving);
                break;
        }
    }

    private void EnterAbility4()
    {
        Debug.Log("Entered ability4");
        navMeshAgent.isStopped = true;
        ability4.TriggerAbility();
    }
    private void UpdateAbility4()
    {
        
    }
    private void ExitAbility4()
    {
        
    }
    private void UpdateAbility4OnActionReceived()
    {
        if (ability4.isComplete) ChangeState(State.Idle);
    }

    private void EnterStunned()
    {
        
    }
    private void UpdateStunned()
    {
        
    }
    private void ExitStunned()
    {
        
    }
    private void UpdateStunnedOnActionReceived()
    {

        switch (actionBuffers.DiscreteActions[0])
        {
            case a_idle:
                ChangeState(State.Idle);
                break;
            case a_moving:
                ChangeState(State.Moving);
                break;
            case a_ability1:
                ChangeState(State.Ability1);
                break;
            case a_ability2:
                ChangeState(State.Ability2);
                break;
            case a_ability3:
                ChangeState(State.Ability3);
                break;
            case a_ability4:
                ChangeState(State.Ability4);
                break;

        }
    }

    private void EnterDead()
    {
        navMeshAgent.enabled = false;
    }
    private void ExitDead()
    {
        
    }
    private void UpdateDead()
    {
        
    }
    private void UpdateDeadOnActionReceived()
    {
        SetReward(0f);
        EndEpisode();
    }


    private void OnTriggerEnter(Collider other)
    {

    }

    void randomizePosition()
    {
        
    }

    private void OnDrawGizmos()
    {
        
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            ChangeState(State.Dead);
        }
    }

    [ContextMenu("Stun the agent")]
    public void Stun()
    {
        isStunned = true;
        ChangeState(State.Stunned);
    }
    public EntityType GetEntityType()
    {
        return entity;
    }
}
