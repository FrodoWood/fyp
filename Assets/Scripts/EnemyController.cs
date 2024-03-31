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
    public Vector3 currentHeuristicDestinationDirection;
    public float currentHeuristicDestinationMagnitude;

    private Bullet[] bullets;
    private BufferSensorComponent bufferSensor;
    Collider coll;
    public Transform goal;
    public EnemyController targetEnemy;
    public Transform enemyGoal;

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

    public Ability1 ability1;
    public Ability2 ability2;
    public Ability3 ability3;
    public Ability4 ability4;

    [Header("States")]
    public bool isAIControlled = false;
    public bool hasWon = false;
    public bool isAlive = true;

    [Header("Statistics")]
    public int score;

    //Cached Inputs
    private bool mouseButtonPressed = false;
    private KeyCode abilityKeyCode = KeyCode.None;

    public AnimationCurve goalRewardCurve;
    public AnimationCurve aimRewardCurve;

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
        //isStunned = false;
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
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);

        sensor.AddObservation(targetEnemy.currentHealth / 80f);
        //sensor.AddObservation(currentHealth / 80f);
        sensor.AddObservation(targetEnemy.isAlive ? 1 : 0);


        Vector3 relativeDistanceToTarget = (targetEnemy.transform.position - transform.position);
        Vector3 directionToTarget = relativeDistanceToTarget.normalized;
        //Debug.DrawLine(Vector3.zero, directionToTarget, Color.green);
        float distanceMagnitudeToTarget = relativeDistanceToTarget.magnitude / 85f;

        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.z);
        sensor.AddObservation(distanceMagnitudeToTarget);        
        
        Vector3 relativeDistanceToGoal = (goal.position - transform.position);
        Vector3 directionToGoal = relativeDistanceToGoal.normalized;
        //Debug.DrawLine(Vector3.zero, directionToGoal, Color.green);
        float distanceMagnitudeToGoal = relativeDistanceToGoal.magnitude / 85f;

        sensor.AddObservation(directionToGoal.x);
        sensor.AddObservation(directionToGoal.z);
        sensor.AddObservation(distanceMagnitudeToGoal);

        sensor.AddObservation(Vector3.Distance(targetEnemy.transform.position, enemyGoal.position)/85f);

        ////Debug.DrawLine(transform.position, targetEnemy.transform.position, Color.red);
        ////Debug.DrawLine(transform.position, transform.position + directionToTarget, Color.green, 2f);

        sensor.AddObservation(ability1.Available() ? 1 : 0);

        sensor.AddObservation(navMeshAgent.speed / 100f);
        sensor.AddObservation(targetEnemy.navMeshAgent.speed / 100f);
        //sensor.AddObservation((float) StepCount / MaxStep);


        // Variable length observation
        bullets = transform.parent.GetComponentsInChildren<Bullet>()
            .Where(bullet => bullet.entity != entity)
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
                agentToBullet.magnitude / 85f,
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
        //AddReward(-1f / MaxStep);
        if (actionTimer >= actionCooldown)
        {

            UpdateStateOnActionReceived();
            actionTimer = 0f;
        }

        // DENSE REWARDS
        float distanceToGoal = Vector3.Distance(transform.position, goal.position);
        float distanceToGoalReward = goalRewardCurve.Evaluate(distanceToGoal / 72) * 0.01f;
        AddReward(distanceToGoalReward);

        //if(entity == EntityType.Player)
        //{
        //    //Debug.Log(GetCumulativeReward());
        //    Debug.Log("distance: " + distanceToGoal);
        //    Debug.Log("reward: " + distanceToGoalReward);
        //}

        //var continuousActions = actions.ContinuousActions;
        //Vector3 actionDirection = new Vector3(continuousActions[0], 0f, continuousActions[1]).normalized;
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

    public int GetAbilityAction(KeyCode keycode)
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

    public void ChangeState(State newState)
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
            if(Random.value < 0.2f && ability1.Available())
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);
                if (distanceToTarget < 25) ChangeState(State.Ability1);
                return;
            }
            if (navMeshAgent.isActiveAndEnabled)
            {
                if (navMeshAgent.remainingDistance < 0.02f || Random.value < 0.05f) ChangeState(State.Moving);
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
        if(navMeshAgent != null && navMeshAgent.isActiveAndEnabled) navMeshAgent.isStopped = false;
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
            if(navMeshAgent.isActiveAndEnabled && !targetEnemy.isAlive)
            {
                navMeshAgent.SetDestination(goal.position);
                ChangeState(State.Idle);
                return;
            }
            Vector3 randomDirection = GetRandomPositionInCircle(1);
            Vector3 targetDirection = (targetEnemy.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);

            float targetBias = distanceToTarget <= 15f? 0f : 0.4f;
            Vector3 finalDirection = Vector3.Lerp(randomDirection, targetDirection, targetBias).normalized;
            Vector3 destination = transform.position + (finalDirection * 5f);
            Vector3 finalDestination = new Vector3(destination.x, transform.position.y, destination.z);
            if (navMeshAgent.isActiveAndEnabled) navMeshAgent.SetDestination(finalDestination);
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
        Vector3 actionDestinationWorldOrigin = new Vector3(Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f), 0f, Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f)).normalized * destinationMagnitude;
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
        if (isAIControlled)
        {
            Vector3 randomPos = GetRandomPositionInCircle(2);
            Vector3 directionToTarget = (targetEnemy.transform.position + randomPos - transform.position).normalized;
            //Debug.DrawLine(Vector3.zero, directionToTarget, Color.green);
            Vector3 lookAtTarget = new Vector3(directionToTarget.x, 0f, directionToTarget.z).normalized + transform.position;
            transform.LookAt(lookAtTarget);
            ability1.TriggerAbility();

        }
        else
        {
            //Debug.Log("Entered ability1");
            //navMeshAgent.isStopped = true;
            Vector3 lookAtTarget = new Vector3(Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f), 0f, Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f)).normalized + transform.position;
            transform.LookAt(lookAtTarget);

            // DENSE REWARDS
            Vector3 trueDirectionTarget = (targetEnemy.transform.position - transform.position).normalized;
            float alignment = Vector3.Dot((lookAtTarget - transform.position).normalized, trueDirectionTarget);
            float aimReward = aimRewardCurve.Evaluate(alignment) * 0.2f;
            if (targetEnemy.isAlive) AddReward(aimReward);

            ability1.TriggerAbility();
            
            //if (entity == EntityType.Player)
            //{
            //    //Debug.Log(GetCumulativeReward());
            //    Debug.Log("alignment: " + alignment);
            //    Debug.Log("reward: " + aimReward);
            //}
        }
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
        if(coll != null) coll.enabled = false;
        if(navMeshAgent != null) navMeshAgent.enabled = false;
        if(targetEnemy != null) targetEnemy.navMeshAgent.speed = 20f;
        targetEnemy?.AddScore(5);
        targetEnemy?.AddReward(1f);

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
            currentHealth = 0;
            ChangeState(State.Dead);
        }
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
        if (Physics.Raycast(ray, out hit, 300f, movementLayers))
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
        if (navMeshAgent == null) yield return null;
        navMeshAgent.speed -= 5;
        yield return new WaitForSeconds(1);
        navMeshAgent.speed += 5;
    }
    public IEnumerator SpeedUp()
    {
        if (navMeshAgent == null) yield return null;
        navMeshAgent.speed += 5;
        yield return new WaitForSeconds(1);
        navMeshAgent.speed -= 5;
    }

    public Vector3 GetRandomPositionInCircle(float radius)
    {
        //float angle = Random.Range(0f, Mathf.PI * 2f);
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float x = (Mathf.PerlinNoise((angle + Time.time) * 0.8f, Time.time) * 2f -1) * radius;
        float z = (Mathf.PerlinNoise(Time.time, (angle + Time.time) * 0.8f) * 2f -1) * radius;
        float y = 0;
        return new Vector3(x, y, z);
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void SetBehaviourType(BehaviorType behaviourType)
    {
        behaviorParameters.BehaviorType = behaviourType;
    }

    public void EnableAllAbilities()
    {
        ability1.EnableAbility();
    }

    public void DisableAllAbilities()
    {
        ability1.DisableAbility();
    }
}
