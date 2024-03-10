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

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
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
            actionBuffers = actions;
            UpdateStateOnActionReceived(actions);

            //switch (currentState)
            //{
            //    case State.Idle:
                    
            //        break;
            //    case State.Moving:
            //        float xDestination = actions.ContinuousActions[0] * 15f;
            //        float zDestination = actions.ContinuousActions[1] * 15f;
            //        navMeshAgent.SetDestination(new Vector3(xDestination, 0f, zDestination));
            //        Debug.Log("Model chose destination");
            //        break;
            //    case State.Stunned:
                    
            //        break;
            //    case State.Ability1:
                    
            //        break;
            //    case State.Ability2:
                    
            //        break;
            //    case State.Ability3:
                    
            //        break;
            //    case State.Ability4:
                    
            //        break;
            //    case State.Dead:
                    
            //        break;
            //}

            actionTimer = 0f;
        }
        
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
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
    private void UpdateStateOnActionReceived(ActionBuffers actions)
    {
        switch (currentState)
        {
            case State.Idle:
                UpdateIdleOnActionReceived(actions);
                break;
            case State.Moving:
                UpdateMovingOnActionReceived(actions);
                break;
            case State.Stunned:
                UpdateStunnedOnActionReceived(actions);
                break;
            case State.Ability1:
                UpdateAbility1OnActionReceived(actions);
                break;
            case State.Ability2:
                UpdateAbility2OnActionReceived(actions);
                break;
            case State.Ability3:
                UpdateAbility3OnActionReceived(actions);
                break;
            case State.Ability4:
                UpdateAbility4OnActionReceived(actions);
                break;
            case State.Dead:
                UpdateDeadOnActionReceived(actions);
                break;
        }
    }


    private void EnterIdle()
    {

    }
    private void UpdateIdle()
    {
        
    }
    private void ExitIdle()
    {
        
    }
    private void UpdateIdleOnActionReceived(ActionBuffers actions)
    {
        switch (actions.DiscreteActions[0])
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
        
    }
    private void UpdateMoving()
    {

    }
    private void ExitMoving()
    {
        
    }
    private void UpdateMovingOnActionReceived(ActionBuffers actions)
    {
        float xDestination = actionBuffers.ContinuousActions[0] * 15f;
        float zDestination = actionBuffers.ContinuousActions[1] * 15f;
        navMeshAgent.SetDestination(new Vector3(xDestination, 0f, zDestination));
        Debug.Log("Model chose destination");

        switch (actions.DiscreteActions[0])
        {
            case a_idle:
                ChangeState(State.Idle);
                break;
            case a_moving:
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

    private void EnterAbility1()
    {
        
    }
    private void UpdateAbility1()
    {
        
    }
    private void ExitAbility1()
    {
        
    }
    private void UpdateAbility1OnActionReceived(ActionBuffers actions)
    {
        
    }

    private void EnterAbility2()
    {
        
    }
    private void UpdateAbility2()
    {
        
    }
    private void ExitAbility2()
    {
        
    }
    private void UpdateAbility2OnActionReceived(ActionBuffers actions)
    {
        
    }

    private void EnterAbility3()
    {
        
    }
    private void UpdateAbility3()
    {
        
    }
    private void ExitAbility3()
    {
        
    }
    private void UpdateAbility3OnActionReceived(ActionBuffers actions)
    {
        
    }

    private void EnterAbility4()
    {
        
    }
    private void UpdateAbility4()
    {
        
    }
    private void ExitAbility4()
    {
        
    }
    private void UpdateAbility4OnActionReceived(ActionBuffers actions)
    {
        
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
    private void UpdateStunnedOnActionReceived(ActionBuffers actions)
    {

        switch (actions.DiscreteActions[0])
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
        
    }
    private void ExitDead()
    {
        
    }
    private void UpdateDead()
    {
        
    }
    private void UpdateDeadOnActionReceived(ActionBuffers actions)
    {
        
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

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            SetReward(0);
            EndEpisode();
        }
    }

    public EntityType GetEntityType()
    {
        return entity;
    }
}
