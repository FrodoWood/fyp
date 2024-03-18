using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour, ICollectable
{
    public EntityType entityType;
    public void OnCollect(EnemyController enemyController)
    {
        if(enemyController.GetEntityType() == entityType)
        {
            enemyController.hasWon = true;
        }
    }
}
