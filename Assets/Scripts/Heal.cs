using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour, ICollectable
{
    public int healAmount = 10;
    public void OnCollect(EnemyController enemyController)
    {
        enemyController.AddHealth(healAmount);
        enemyController.StartCoroutine(enemyController.SpeedUp());
        //enemyController?.AddReward(0.5f);
        enemyController?.AddScore(1);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }


}
