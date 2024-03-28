using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Linq;


public class EnemyControllerPlayTest
{
    EnemyController[] enemyControllers;
    EnemyController agent1;
    EnemyController agent2;

    [OneTimeSetUp]
    public void LoadScene()
    {
        EditorSceneManager.LoadScene(0);
    }


    [UnityTest]
    public IEnumerator ReachGoalTest()
    {
        float maxWaitTime = 10f;
        float startTime = Time.time;

        enemyControllers = Object.FindObjectsOfType<EnemyController>();
        Debug.Log("Number of enemy controllers" + enemyControllers.Length);
        agent1 = enemyControllers[0];
        agent2 = enemyControllers[1];

        agent1.baseSpeed = 0;
        agent2.navMeshAgent.speed = 0;
        agent1.SetBehaviourType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        agent1.isAIControlled = false;
        agent1.DisableAllAbilities();

        agent2.baseSpeed = 10;
        agent2.navMeshAgent.speed = 10f;
        agent2.SetBehaviourType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        agent2.isAIControlled = false;

        agent2.StopAllCoroutines();

        while (Time.time -  startTime < maxWaitTime)
        {
            agent2.ChangeState(State.Moving);
            agent2.currentHeuristicDestinationDirection = (agent2.goal.position - agent2.transform.position).normalized;    
            agent2.currentHeuristicDestinationMagnitude = 10;
            float distanceToGoal = Vector3.Distance(agent2.transform.position, agent2.goal.position);
            if(distanceToGoal < 5f)
            {
                yield break;
            }
            yield return null;
        }

        Assert.Fail("Agent didn't reach the goal in time.");
    }

    [UnityTest]
    public IEnumerator HealCollectableTest()
    {
        float maxWaitTime = 5f;
        float startTime = Time.time;

        enemyControllers = Object.FindObjectsOfType<EnemyController>();
        Debug.Log("Number of enemy controllers" + enemyControllers.Length);
        agent1 = enemyControllers[0];
        agent2 = enemyControllers[1];

        agent1.baseSpeed = 0;
        agent2.navMeshAgent.speed = 0;
        agent1.SetBehaviourType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        agent1.isAIControlled = false;
        agent1.DisableAllAbilities();

        agent2.baseSpeed = 10;
        agent2.navMeshAgent.speed = 10f;
        agent2.SetBehaviourType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        agent2.isAIControlled = false;

        // Get the nearest heal
        Heal[] heals;
        heals = agent2.transform.parent.GetComponentsInChildren<Heal>()
            .OrderBy(heal => Vector3.Distance(agent2.transform.localPosition, heal.transform.localPosition))
            .Take(1)
            .ToArray();

        float initialHealth = agent2.currentHealth;
        float initialMoveSpeed = agent2.navMeshAgent.speed;
        float initialScore = agent2.score;

        agent2.StopAllCoroutines();
        // Test
        while (Time.time - startTime < maxWaitTime)
        {
            agent2.ChangeState(State.Moving);
            agent2.currentHeuristicDestinationDirection = (heals[0].transform.position - agent2.transform.position).normalized;
            agent2.currentHeuristicDestinationMagnitude = 10;
            float distanceToHeal = Vector3.Distance(agent2.transform.position, heals[0].transform.position);
            if (agent2.currentHealth > initialHealth && agent2.navMeshAgent.speed > initialMoveSpeed && agent2.score > initialScore)
            {
                yield break;
            }
            yield return null;
        }

        Assert.Fail("Agent didn't receive health or movement speed upon collecting the heal");
    }

    [UnityTest]
    public IEnumerator TakeDamageSlowDownAndRewardTest()
    {
        float maxWaitTime = 10f;
        float startTime = Time.time;

        enemyControllers = Object.FindObjectsOfType<EnemyController>();
        Debug.Log("Number of enemy controllers" + enemyControllers.Length);
        agent1 = enemyControllers[0];
        agent2 = enemyControllers[1];

        agent1.baseSpeed = 10;
        agent2.navMeshAgent.speed = 10;
        agent1.SetBehaviourType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        agent1.isAIControlled = true;
        agent1.EnableAllAbilities();

        agent2.baseSpeed = 10;
        agent2.navMeshAgent.speed = 10f;
        agent2.SetBehaviourType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        agent2.isAIControlled = false;

        agent2.StopAllCoroutines();

        float initialHealth = agent2.currentHealth;
        float initialMoveSpeed = agent2.navMeshAgent.speed;
        float currentCumulativeReward = agent2.GetCumulativeReward();

        while (Time.time - startTime < maxWaitTime)
        {
            if(agent2.navMeshAgent.isActiveAndEnabled) agent2.ChangeState(State.Moving);
            agent2.currentHeuristicDestinationDirection = (agent1.transform.position - agent2.transform.position).normalized;
            agent2.currentHeuristicDestinationMagnitude = 10;
            if (agent2.currentHealth < initialHealth && agent2.navMeshAgent.speed < initialMoveSpeed && agent2.GetCumulativeReward() < currentCumulativeReward)
            {
                yield break;
            }
            yield return null;
        }

        Assert.Fail("Agent didn't get attacked by the hard coded enemy AI, or agent wasn't slowed, or agent's didn't receive negative reward.");
    }

}
