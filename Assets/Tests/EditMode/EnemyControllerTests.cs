using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyControllerTests
{
    EnemyController agent;
    [SetUp]
    public void Setup()
    {
        agent = new GameObject().AddComponent<EnemyController>();
        agent.maxHealth = 10f;
    }

    [Test]
    public void AddScoreTest()
    {
        int score = agent.score;

        int amountToAdd = 10;
        agent.AddScore(amountToAdd); 

        int expectedScore = score + amountToAdd;
        Assert.AreEqual(expectedScore, agent.score);
    }

    [Test]
    public void ChangeStateTest()
    {
        agent.ChangeState(State.Idle);
        State initialState = agent.currentState;
        Assert.AreEqual(State.Idle, initialState);

        agent.ChangeState(State.Moving);
        State newState = agent.currentState;
        Assert.AreEqual(State.Moving, newState);

        Assert.AreNotEqual(initialState, newState);
    }

    [Test]
    public void TakeDamageTest()
    {
        agent.currentHealth = 10f;
        agent.TakeDamage(4);
        Assert.AreEqual(agent.currentHealth, 6f);
        agent.TakeDamage(10);
        Assert.AreNotEqual(agent.currentHealth, -4f);
        Assert.AreEqual(agent.currentHealth, 0f);

    }

    [Test]
    public void ResetHealthTest()
    {
        agent.currentHealth = 5;
        Assert.AreNotEqual(agent.currentHealth, agent.maxHealth);
        agent.ResetHealth();
        Assert.AreEqual(agent.currentHealth, agent.maxHealth);
    }
    
    [Test]
    public void AddHealthTest()
    {
        agent.currentHealth = 5;
        agent.AddHealth(5);
        Assert.AreEqual(agent.currentHealth, 10);
        agent.AddHealth(0);
        Assert.AreEqual(agent.currentHealth, 10);
        agent.AddHealth(-5);
        Assert.AreEqual(agent.currentHealth, 10);
    }

    [Test]
    public void GetRandomPositionInCircleTest()
    {
        Vector3 randomPos1 = agent.GetRandomPositionInCircle(1);
        Vector3 randomPos2 = agent.GetRandomPositionInCircle(10);
        Assert.AreNotEqual(randomPos1, randomPos2);
    }

    [Test]
    public void GetAbilityActionTest()
    {
        int ability1 = agent.GetAbilityAction(KeyCode.Q);
        int ability2 = agent.GetAbilityAction(KeyCode.W);
        int ability3 = agent.GetAbilityAction(KeyCode.E);
        int ability4 = agent.GetAbilityAction(KeyCode.R);

        Assert.AreEqual(ability1, 2);
        Assert.AreEqual(ability2, 3);
        Assert.AreEqual(ability3, 4);
        Assert.AreEqual(ability4, 5);
    }


}
