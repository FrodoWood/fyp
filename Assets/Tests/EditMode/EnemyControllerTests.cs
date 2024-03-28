using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyControllerTests
{
    [Test]
    public void AddScoreTest()
    {
        EnemyController enemyController = new GameObject().AddComponent<EnemyController>();
        int score = enemyController.score;

        int amountToAdd = 10;
        enemyController.AddScore(amountToAdd); 

        int expectedScore = score + amountToAdd;
        Assert.AreEqual(expectedScore, enemyController.score);
    }

}
