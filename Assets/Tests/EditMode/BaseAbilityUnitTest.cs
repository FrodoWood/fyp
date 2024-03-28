using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BaseAbilityUnitTest
{
    EnemyController agent;
    Ability1 ability1;

    [SetUp]
    public void Setup()
    {
        agent = new GameObject().AddComponent<EnemyController>();
        agent.gameObject.AddComponent<Ability1>();
        ability1 = agent.GetComponent<Ability1>();
        agent.maxHealth = 10f;
    }

    [Test]
    public void EnableAbilityTest()
    {
        Assert.IsFalse(ability1.isEnabled());
        ability1.EnableAbility();
        Assert.IsTrue(ability1.isEnabled());
    }
    
    [Test]
    public void DisableAbilityTest()
    {
        Assert.IsFalse(ability1.isEnabled());
        ability1.EnableAbility();
        Assert.IsTrue(ability1.isEnabled());
        ability1.DisableAbility();
        Assert.IsFalse(ability1.isEnabled());
    }
}
