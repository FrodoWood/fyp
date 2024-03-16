using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using UnityEngine.AI;
using System;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;


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
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentHealth;
    [SerializeField] private bool isStunned;
    [SerializeField] private LayerMask movementLayers;
    public EnemyController targetEnemy;
    private float actionTimer = 0f;
    public State currentState { get; private set; }
    private Vector3 currentHeuristicDestinationDirection;
    private float currentHeuristicDestinationMagnitude;

    private Bullet[] bullets;
    private BufferSensorComponent bufferSensor;

    [Header("Gizmos")]
    public float bulletGizmoRadius = 1f;
    public Color bulletGizmoColor;

    const int a_idle = 0;
    const int a_moving = 1;
    const int a_ability1 = 2;
    const int a_ability2 = 3;
    const int a_ability3 = 4;
    const int a_ability4 = 5;

    private ActionBuffers actionBuffers;
    private BehaviorParameters behaviorParameters;

    private Ability1 ability1;
    private Ability2 ability2;
    private Ability3 ability3;
    private Ability4 ability4;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
        behaviorParameters = GetComponent<BehaviorParameters>();
        ability1 = GetComponent<Ability1>();
        ability2 = GetComponent<Ability2>();
        ability3 = GetComponent<Ability3>();
        ability4 = GetComponent<Ability4>();
    }

    private void Start()
    {
        ChangeState(State.Idle);
        ResetHealth();
    }

    private void Update()
    {
        actionTimer += Time.deltaTime;
        SetCurrentDestination();
        UpdateCurrentState();
        //Debug.Log($" Navmesh destination: {navMeshAgent.destination}");
    }

    public override void Initialize()
    {
        ChangeState(State.Idle);
        isStunned = false;
        bullets = FindObjectsOfType<Bullet>();
        bufferSensor = GetComponent<BufferSensorComponent>();
    }

    public override void OnEpisodeBegin()
    {
        ChangeState(State.Idle);
        if(navMeshAgent.hasPath) navMeshAgent.ResetPath();
        
        currentHeuristicDestinationDirection = transform.position.normalized;
        currentHeuristicDestinationMagnitude = transform.position.magnitude;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(transform.localPosition.x / 15f);
        //sensor.AddObservation(transform.localPosition.z / 15f);
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);

        sensor.AddObservation(targetEnemy.currentHealth / targetEnemy.maxHealth);
        //sensor.AddObservation(targetEnemy.transform.localPosition.x / 15f);
        //sensor.AddObservation(targetEnemy.transform.localPosition.z / 15f);

        Vector3 relativeDistanceToTarget = (targetEnemy.transform.position - transform.position);
        Vector3 directionToTarget = relativeDistanceToTarget.normalized;
        float distanceMagnitudeToTarget = relativeDistanceToTarget.magnitude;

        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.z);
        sensor.AddObservation(distanceMagnitudeToTarget);

        Debug.DrawLine(transform.position, targetEnemy.transform.position, Color.red);
        Debug.DrawLine(transform.position, transform.position + directionToTarget, Color.green);

        sensor.AddObservation(ability1.Available() ? 1 : 0);


        // Variable length observation
        bullets = transform.parent.GetComponentsInChildren<Bullet>()
            //.Where(bullet=> bullet.entity != entity)
            .OrderBy(bullet => Vector3.Distance(transform.localPosition, bullet.transform.localPosition))
            .Take(3)
            .ToArray();

        foreach (Bullet bullet in bullets)
        {
            float[] bulletObservation = new float[]
            {
                (bullet.transform.localPosition.x - transform.localPosition.x) / 15f,
                (bullet.transform.localPosition.z - transform.localPosition.z) / 15f,
                bullet.transform.forward.x,
                bullet.transform.forward.z
            };
            //Debug.Log("\n" + bulletObservation[0] + bulletObservation[1]);
            bufferSensor.AppendObservation(bulletObservation);

        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        actionBuffers = actions;
        AddReward(-0.1f / MaxStep);
        if (actionTimer >= actionCooldown)
        {

            UpdateStateOnActionReceived();
            actionTimer = 0f;
        }

        //if(StepCount == MaxStep)
        //{
        //    SetReward(-1f);
        //}

    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Setting continuous actions
        continuousActionsOut[0] = currentHeuristicDestinationDirection.x;
        continuousActionsOut[1] = currentHeuristicDestinationDirection.z;

        // Setting discrete actions
        if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = a_idle;
        else if (Input.GetKey(KeyCode.Q)) discreteActionsOut[0] = a_ability1;
        else if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = a_ability2;
        else if (Input.GetKey(KeyCode.E)) discreteActionsOut[0] = a_ability3;
        else if (Input.GetKey(KeyCode.R)) discreteActionsOut[0] = a_ability4;
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
        //navMeshAgent.isStopped = true;
        Debug.Log("Entered Idle/No Input");
    }
    private void UpdateIdle()
    {
        
    }
    private void ExitIdle()
    {
        //navMeshAgent.isStopped = false;
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
                if (ability1.Available()) ChangeState(State.Ability1);
                break;

            case a_ability2:
                if (ability2.Available()) ChangeState(State.Ability2);
                break;

            case a_ability3:
                if (ability3.Available()) ChangeState(State.Ability3);
                break;

            case a_ability4:
                if (ability4.Available()) ChangeState(State.Ability4);
                break;
        }
    }

    private void EnterMoving()
    {
        navMeshAgent.isStopped = false;
        Debug.Log("Entered Moving");
    }
    private void UpdateMoving()
    {

    }
    private void ExitMoving()
    {
        
    }
    private void UpdateMovingOnActionReceived()
    {
        float destinationMagnitude;
        if (behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly) destinationMagnitude = currentHeuristicDestinationMagnitude;
        else destinationMagnitude = 4f;

        float yEnvironment = transform.parent.position.y;

        Vector3 actionDestinationWorldOrigin = new Vector3(actionBuffers.ContinuousActions[0], 0f, actionBuffers.ContinuousActions[1]).normalized * destinationMagnitude;
        Vector3 actionDestinationLocalOriginWorldPosition = actionDestinationWorldOrigin + transform.position;

        //Debug.DrawLine(Vector3.zero, actionDestinationWorldOrigin, Color.green);
        //Debug.DrawLine(transform.position, actionDestinationLocalOriginWorldPosition, Color.red);

        navMeshAgent.SetDestination(actionDestinationLocalOriginWorldPosition);
        //Debug.Log($"Training env pos: {transform.parent.position}");
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
                if (ability2.Available()) ChangeState(State.Ability2);
                break;

            case a_ability3:
                if (ability3.Available()) ChangeState(State.Ability3);
                break;

            case a_ability4:
                if (ability4.Available()) ChangeState(State.Ability4);
                break;
        }
    }

    private void EnterAbility1()
    {
        if (!ability1.isEnabled()) return;
        Debug.Log("Entered ability1");
        navMeshAgent.isStopped = true;
        Vector3 lookAtTarget = new Vector3(actionBuffers.ContinuousActions[0], 0f, actionBuffers.ContinuousActions[1]).normalized + transform.position;
        transform.LookAt(lookAtTarget);
        //Vector3 lookAtTarget = Vector3.zero;
        //if (behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly)
        //{
        //    lookAtTarget = new Vector3(currentHeuristicDestinationDirection.x, transform.position.y, currentHeuristicDestinationDirection.z) * currentHeuristicDestinationMagnitude * 1.2f;
        //    transform.LookAt(lookAtTarget);

        //}
        //else
        //{
        //    lookAtTarget = new Vector3(actionBuffers.ContinuousActions[0], 0f, actionBuffers.ContinuousActions[1]).normalized + transform.position;
        //}

        ability1.TriggerAbility();
    }
    private void UpdateAbility1()
    {
        
    }
    private void ExitAbility1()
    {
        navMeshAgent.isStopped = false;
    }
    private void UpdateAbility1OnActionReceived()
    {
        ChangeState(State.Moving);
    }

    private void EnterAbility2()
    {
        if (!ability2.isEnabled()) return;
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
        ChangeState(State.Moving);

    }

    private void EnterAbility3()
    {
        if (!ability3.isEnabled()) return;
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
        ChangeState(State.Moving);
    }

    private void EnterAbility4()
    {
        if (!ability4.isEnabled()) return;
        Debug.Log("Entered ability4");
        navMeshAgent.isStopped = true;
        ability4.TriggerAbility();
    }
    private void UpdateAbility4()
    {
        
    }
    private void ExitAbility4()
    {
        Debug.Log("Exited Ability 4");
    }
    private void UpdateAbility4OnActionReceived()
    {
        if (ability4.isComplete || !ability4.isEnabled()) ChangeState(State.Idle);
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
                if (ability1.Available()) ChangeState(State.Ability1);
                break;

            case a_ability2:
                if (ability2.Available()) ChangeState(State.Ability2);
                break;

            case a_ability3:
                if (ability3.Available()) ChangeState(State.Ability3);
                break;

            case a_ability4:
                if (ability4.Available()) ChangeState(State.Ability4);
                break;
        }
    }

    private void EnterDead()
    {
        SetReward(-1f);
    }
    private void ExitDead()
    {
        
    }
    private void UpdateDead()
    {
        
    }
    private void UpdateDeadOnActionReceived()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {

    }

    Vector3 getRandomPosition(Vector3 currentPosition)
    {
        return new Vector3((Random.value * 28) - 14f, currentPosition.y, (Random.value * 28) - 14f);
    }

    private void OnDrawGizmos()
    {
        if (bullets != null && bullets.Length > 0)
        {
            foreach (Bullet bullet in bullets)
            {
                if (bullet == null) continue;
                Gizmos.color = bulletGizmoColor;
                Gizmos.DrawSphere(bullet.transform.position, bulletGizmoRadius);
            }
        }
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

    private void SetCurrentDestination()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 1f);
            RaycastHit hit;
            //Vector3 movePosition = Vector3.zero;
            if (Physics.Raycast(ray, out hit, 100f, movementLayers))
            {
                Vector3 destination = hit.point;
                currentHeuristicDestinationDirection = destination.normalized;
                currentHeuristicDestinationMagnitude = destination.magnitude;
            }
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}
