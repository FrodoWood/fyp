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
    public BufferSensorComponent bufferSensor;
    Vector3 mousePos;
    Vector3 movement;
    public Transform player;
    public Transform ball;
    public Transform gun;
    private Enemy enemy;
    
    private List<Bullet> bullets;

    void Start()
    {
        bufferSensor = GetComponent<BufferSensorComponent>();
        controller = GetComponent<CharacterController>();
        enemy = GetComponent<Enemy>();
        bullets = new List<Bullet>();
    }

    public override void Initialize()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        

        movement = Vector3.zero;
        // move target (Player) to a new random position
        player.localPosition = new Vector3(Random.value * 20 - 10, 0f, Random.value * 20 - 10);
        // move agent to random new position
        randomizePosition();
        enemy.resetHealth();
        bullets.Clear();
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

        
        
        Debug.Log(bullets.Count + "bullets observed");
        foreach (Bullet bullet in bullets.Take(10))
        {
            Vector2 bulletRelativeDistance = new Vector2(bullet.transform.position.x - transform.position.x, bullet.transform.position.z - transform.position.z);
            bulletRelativeDistance /= 10f;
            float[] bulletRelativeDistanceArray = new float[] { bulletRelativeDistance.x * 2f - 1f, bulletRelativeDistance.y * 2f - 1f };
            Debug.Log("relative distance array " + bulletRelativeDistanceArray.Length + "content:" + bulletRelativeDistanceArray.ToString());
            bufferSensor.AppendObservation(bulletRelativeDistanceArray);

            //Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            //if (bulletRigidbody != null)
            //{
            //    Vector2 bulletVelocity = new Vector2(bulletRigidbody.velocity.x, bulletRigidbody.velocity.z);
            //    bulletVelocity.Normalize();
            //    float[] bulletVelocityArray = new float[] { bulletVelocity.x * 2f - 1f, bulletVelocity.y * 2f - 1f };
            //    Debug.Log("velocity array length " + bulletVelocityArray.Length);
            //    bufferSensor.AppendObservation(bulletVelocityArray);
            //}
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        controller.Move(controlSignal * Time.deltaTime * moveSpeed);



        // Rewards for chase
        //float distanceToTarget = Vector3.Distance(this.transform.localPosition, player.localPosition);
        //if(distanceToTarget < 1.5f)
        //{
        //    SetReward(1f);
        //    EndEpisode();
        //}
        //else if(this.transform.localPosition.x > 15f || this.transform.localPosition.x < -15f || this.transform.localPosition.z > 15f || this.transform.localPosition.z < -15f)
        //{
        //    SetReward(-0.5f);
        //    randomizePosition();
        //    EndEpisode();
        //}
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
  
    void Update()
    {
        bullets = GameObject.FindObjectsOfType<Bullet>().ToList<Bullet>();
        bullets = bullets.Where(bullet => Vector3.Distance(transform.position, bullet.transform.position) <= 10f).ToList();
        bullets = bullets.OrderBy(bullet => Vector3.Distance(transform.position, bullet.transform.position)).ToList();

        if (this.StepCount == this.MaxStep)
        {
            Debug.Log("End of episode conditions are met, max step count reached!");
            if (enemy.isAlive())
            {
                SetReward(1f);
            }
            Debug.Log("Ending the episode...");
            EndEpisode();
        }
    }

    private new void OnEnable()
    {
        Enemy.onEnemyHit += HandleEnemyHit;
        Enemy.onEnemyDeath += HandleEnemyDeath;
    }

    private new void OnDisable()
    {
        Enemy.onEnemyHit -= HandleEnemyHit;
        Enemy.onEnemyDeath -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath()
    {
        Debug.Log("Enemy has died!");
        SetReward(-1);
        EndEpisode();

    }

    private void HandleEnemyHit(float damageAmount)
    {
        Debug.Log("Enemy has been hit!");
        SetReward(-0.2f);
    }

    void randomizePosition()
    {
        this.transform.localPosition = new Vector3(Random.value * 20 - 10, 0f, Random.value * 20 - 10);
        //this.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    

}
