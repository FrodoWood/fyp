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
    public NavMeshAgent navMeshAgent { get; private set; }
    [SerializeField] private float actionCooldown;
    [SerializeField] public float maxHealth;
    [SerializeField] public float currentHealth;
    [SerializeField] public float baseSpeed;
    [SerializeField] private LayerMask movementLayers;
    private float actionTimer = 0f;
    public State currentState { get; private set; }
    private Vector3 currentHeuristicDestinationDirection;
    private float currentHeuristicDestinationMagnitude;

    private Bullet[] bullets;
    private BufferSensorComponent bufferSensor;
    Collider coll;
    public Transform goal;
    public EnemyController targetEnemy;
    public Transform enemyGoal;
    public bool isAIControlled = false;

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

    [SerializeField] private bool isStunned;
    public bool hasWon = false;
    public bool isAlive = true;

    //Cached Inputs
    private bool mouseButtonPressed = false;
    private KeyCode abilityKeyCode = KeyCode.None;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
        behaviorParameters = GetComponent<BehaviorParameters>();
        coll = GetComponent<Collider>();
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
        SetCurrentDestination(); // also caching mouse input
        UpdateCurrentState();
        ////Debug.Log($" Navmesh destination: {navMeshAgent.destination}");

        // Caching inputs because Heuristic gets called every FixedUpdate
        if      (Input.GetKey(KeyCode.Q)) abilityKeyCode =  KeyCode.Q;
        else if (Input.GetKey(KeyCode.W)) abilityKeyCode =  KeyCode.W;
        else if (Input.GetKey(KeyCode.E)) abilityKeyCode =  KeyCode.E;
        else if (Input.GetKey(KeyCode.R)) abilityKeyCode =  KeyCode.R;
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
        hasWon = false;
        isAlive = true;
        coll.enabled = true;
        navMeshAgent.enabled = true;
        navMeshAgent.speed = baseSpeed;

        //if(navMeshAgent.hasPath) navMeshAgent.ResetPath();

        currentHeuristicDestinationDirection = transform.position.normalized;
        currentHeuristicDestinationMagnitude = transform.position.magnitude;
        navMeshAgent.ResetPath();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(transform.localPosition.x / 15f);
        //sensor.AddObservation(transform.localPosition.z / 15f);
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);

        sensor.AddObservation(targetEnemy.currentHealth / targetEnemy.maxHealth);
        sensor.AddObservation(targetEnemy.isAlive ? 1 : 0);
        //sensor.AddObservation(targetEnemy.transform.localPosition.x / 15f);
        //sensor.AddObservation(targetEnemy.transform.localPosition.z / 15f);

        Vector3 relativeDistanceToTarget = (targetEnemy.transform.position - transform.position);
        Vector3 directionToTarget = relativeDistanceToTarget.normalized;
        //Debug.DrawLine(Vector3.zero, directionToTarget, Color.green);
        float distanceMagnitudeToTarget = relativeDistanceToTarget.magnitude / 70f;

        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.z);
        sensor.AddObservation(distanceMagnitudeToTarget);        
        
        Vector3 relativeDistanceToGoal = (goal.position - transform.position);
        Vector3 directionToGoal = relativeDistanceToGoal.normalized;
        //Debug.DrawLine(Vector3.zero, directionToGoal, Color.green);
        float distanceMagnitudeToGoal = relativeDistanceToGoal.magnitude / 70f;

        sensor.AddObservation(directionToGoal.x);
        sensor.AddObservation(directionToGoal.z);
        sensor.AddObservation(distanceMagnitudeToGoal);

        sensor.AddObservation(Vector3.Distance(targetEnemy.transform.position, enemyGoal.position)/70f);

        ////Debug.DrawLine(transform.position, targetEnemy.transform.position, Color.red);
        ////Debug.DrawLine(transform.position, transform.position + directionToTarget, Color.green, 2f);

        sensor.AddObservation(ability1.Available() ? 1 : 0);

        sensor.AddObservation(navMeshAgent.speed / 100f);
        sensor.AddObservation(targetEnemy.navMeshAgent.speed / 100f);


        // Variable length observation
        bullets = transform.parent.GetComponentsInChildren<Bullet>()
            //.Where(bullet=> bullet.entity != entity)
            .OrderBy(bullet => Vector3.Distance(transform.localPosition, bullet.transform.localPosition))
            .Take(3)
            .ToArray();

        foreach (Bullet bullet in bullets)
        {
            Vector3 agentToBullet = bullet.transform.position - transform.position;

            float[] bulletObservation = new float[]
            {
                //(bullet.transform.localPosition.x - transform.localPosition.x) / 15f,
                //(bullet.transform.localPosition.z - transform.localPosition.z) / 15f,
                agentToBullet.normalized.x,
                agentToBullet.normalized.z,
                agentToBullet.magnitude / 70f,
                bullet.transform.forward.x,
                bullet.transform.forward.z
            };
            ////Debug.Log("\n" + bulletObservation[0] + bulletObservation[1]);
            bufferSensor.AppendObservation(bulletObservation);

        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        actionBuffers = actions;
        AddReward(-1f / MaxStep);
        if (actionTimer >= actionCooldown)
        {

            UpdateStateOnActionReceived();
            actionTimer = 0f;
        }
        var continuousActions = actions.ContinuousActions;
        Vector3 actionDirection = new Vector3(continuousActions[0], 0f, continuousActions[1]).normalized;
        //Debug.DrawLine(Vector3.zero, actionDirection, Color.red);

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
        if (isAIControlled)
        {
            if (!navMeshAgent.hasPath) ChangeState(State.Moving);
            return;
        }
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Setting continuous actions
        continuousActionsOut[0] = currentHeuristicDestinationDirection.x;
        continuousActionsOut[1] = currentHeuristicDestinationDirection.z;

        // Setting discrete actions
        if (abilityKeyCode != KeyCode.None) discreteActionsOut[0] = GetAbilityAction(abilityKeyCode);
        else if (mouseButtonPressed) discreteActionsOut[0] = a_moving;
        else discreteActionsOut[0] = a_idle;

        // Reset input cache after it has been processed here in Heuristic
        abilityKeyCode = KeyCode.None;
        mouseButtonPressed = false;

    }

    private int GetAbilityAction(KeyCode keycode)
    {
        switch (keycode)
        {
            case KeyCode.Q: return a_ability1;
            case KeyCode.W: return a_ability2;
            case KeyCode.E: return a_ability3;
            case KeyCode.R: return a_ability4;
            default: return a_idle;
        }
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
        //Debug.Log("Entered Idle/No Input");
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
        if (isAIControlled)
        {
            if (navMeshAgent.isActiveAndEnabled)
            {
                if (navMeshAgent.remainingDistance < 0.2f) ChangeState(State.Moving);
                return;
            }
            return;
        }
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
        if(navMeshAgent.isActiveAndEnabled) navMeshAgent.isStopped = false;
        //Debug.Log("Entered Moving");
    }
    private void UpdateMoving()
    {

    }
    private void ExitMoving()
    {
        
    }
    private void UpdateMovingOnActionReceived()
    {
        if (isAIControlled)
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1,1), transform.position.y, Random.Range(-1,2)).normalized;
            Vector3 targetDirection = (goal.position - transform.position).normalized;
            Vector3 finalDirection = Vector3.Lerp(randomDirection, targetDirection, 0.6f).normalized;
            if(navMeshAgent.isActiveAndEnabled) navMeshAgent.SetDestination(transform.position + (finalDirection * 6f));
            //if(navMeshAgent.isActiveAndEnabled) navMeshAgent.SetDestination(goal.position + new Vector3(0,0, Random.Range(-2f,2)));
            
            ChangeState(State.Idle);
            return;
        }
        float destinationMagnitude;
        // Heuristic
        if (behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly)
        {
            destinationMagnitude = currentHeuristicDestinationMagnitude;
        }
        else destinationMagnitude = 10f;
        // Training or Inference
        //Debug.Log($"contActions[0]: {actionBuffers.ContinuousActions[0]}, contActions[1]: {actionBuffers.ContinuousActions[1]}");
        Vector3 actionDestinationWorldOrigin = new Vector3(actionBuffers.ContinuousActions[0], 0f, actionBuffers.ContinuousActions[1]).normalized * destinationMagnitude;
        Vector3 actionDestinationOriginToPlayer = actionDestinationWorldOrigin + transform.position;

        ////Debug.DrawLine(Vector3.zero, actionDestinationWorldOrigin, Color.green, 2f);
        ////Debug.DrawLine(transform.position, actionDestinationOriginToPlayer, Color.red, 2f);

        navMeshAgent.SetDestination(actionDestinationOriginToPlayer);
        ////Debug.Log($"Training env pos: {transform.parent.position}");
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
        //Debug.Log("Entered ability1");
        //navMeshAgent.isStopped = true;
        Vector3 lookAtTarget = new Vector3(actionBuffers.ContinuousActions[0], 0f, actionBuffers.ContinuousActions[1]).normalized + transform.position;
        transform.LookAt(lookAtTarget);
        ability1.TriggerAbility();
    }
    private void UpdateAbility1()
    {
        
    }
    private void ExitAbility1()
    {
        //navMeshAgent.isStopped = false;
    }
    private void UpdateAbility1OnActionReceived()
    {
        ChangeState(State.Idle);
    }

    private void EnterAbility2()
    {
        if (!ability2.isEnabled()) return;
        //Debug.Log("Entered ability2");
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
        //Debug.Log("Entered ability3");
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
        //Debug.Log("Entered ability4");
        navMeshAgent.isStopped = true;
        ability4.TriggerAbility();
    }
    private void UpdateAbility4()
    {
        
    }
    private void ExitAbility4()
    {
        //Debug.Log("Exited Ability 4");
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
        isAlive = false;
        coll.enabled = false;
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
        
    }


    private void OnTriggerEnter(Collider other)
    {
        ICollectable collectable = other.GetComponent<ICollectable>();
        collectable?.OnCollect(this);
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
        AddReward(-damageAmount/maxHealth);
        StartCoroutine(SlowDown());
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ////Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 1f);
        RaycastHit hit;
        //Vector3 movePosition = Vector3.zero;
        if (Physics.Raycast(ray, out hit, 100f, movementLayers))
        {
            Vector3 destination = hit.point;
            //Debug.Log($"hit.point = {hit.point}");
            ////Debug.DrawLine(Camera.main.transform.position, destination, Color.red, 1f);

            Vector3 agentToPoint = hit.point;

            // Debug vectors

            Vector3 distancePlayerTargetToOrigin = agentToPoint - transform.position;

            Vector3 destinationDirection = distancePlayerTargetToOrigin.normalized;

            float distanceToPointMagnitude = distancePlayerTargetToOrigin.magnitude;


            currentHeuristicDestinationDirection = destinationDirection;
            currentHeuristicDestinationMagnitude = distanceToPointMagnitude;

            if (Input.GetMouseButtonDown(1))
            {
                mouseButtonPressed = true;
                //Debug.DrawLine(Vector3.zero, transform.position, Color.magenta, 2f);
                //Debug.DrawLine(Vector3.zero, hit.point, Color.black, 2f);
                //Debug.DrawLine(transform.position, agentToPoint, Color.blue, 2f);
            }
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void AddHealth(int amount)
    {
        if (amount <= 0) return;
        currentHealth += amount;
    }

    public IEnumerator SlowDown()
    {
        navMeshAgent.speed = baseSpeed / 4;
        yield return new WaitForSeconds(1);
        navMeshAgent.speed = baseSpeed;
    }
    public IEnumerator SpeedUp()
    {
        navMeshAgent.speed = baseSpeed * 1.5f;
        yield return new WaitForSeconds(1);
        navMeshAgent.speed = baseSpeed;
    }
}
