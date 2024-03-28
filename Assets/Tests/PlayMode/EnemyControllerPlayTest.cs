using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class EnemyControllerPlayTest
{
    [OneTimeSetUp]
    public void LoadScene()
    {
        EditorSceneManager.LoadScene(0);
    }

    [UnityTest]
    public IEnumerator EnemyControllerPlayTestWithEnumeratorPasses()
    {
        EnemyController[] enemyControllers;
        enemyControllers = Object.FindObjectsOfType<EnemyController>();
        Debug.Log("Number of enemy controllers" + enemyControllers.Length);
        EnemyController agent1 = enemyControllers[0];
        EnemyController agent2 = enemyControllers[1];


        float maxWaitTime = 10f;
        float startTime = Time.time;

        agent1.baseSpeed = 0;
        agent2.navMeshAgent.speed = 0;
        agent1.SetBehaviourType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        agent1.isAIControlled = false;
        agent1.DisableAllAbilities();

        agent2.baseSpeed = 10;
        agent2.navMeshAgent.speed = 10f;
        agent2.SetBehaviourType(Unity.MLAgents.Policies.BehaviorType.HeuristicOnly);
        agent2.isAIControlled = false;
        while(Time.time -  startTime < maxWaitTime)
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
}
