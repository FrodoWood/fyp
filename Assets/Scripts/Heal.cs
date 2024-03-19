using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour, ICollectable
{
    public int healAmount = 10;
    public void OnCollect(EnemyController enemyController)
    {
        enemyController.AddHealth(healAmount);
        Destroy(gameObject);
    }
}
