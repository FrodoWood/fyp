using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnvController : MonoBehaviour
{
    [SerializeField] private EnemyController purpleAgent;
    [SerializeField] private EnemyController blueAgent;

    public int purpleScore;
    public int blueScore;
    public TextMeshProUGUI scoreText;
    
    

    private void Update()
    {
        updateScoreText();

        if (blueAgent.currentState == State.Dead)
        {
            purpleAgent.AddReward(4f);
            blueAgent.AddReward(-2f);
            increasePurpleScore();
            ResetScene();
        }
        else if (purpleAgent.currentState == State.Dead)
        {
            blueAgent.AddReward(4f);
            purpleAgent.AddReward(-2f);
            increaseBlueScore();
            ResetScene();
        }

        if(purpleAgent.StepCount >= purpleAgent.MaxStep -1 || blueAgent.StepCount >= blueAgent.MaxStep-1)
        {
            //purpleAgent.SetReward(0f);
            //blueAgent.SetReward(0f);
            ResetScene();   
        }
        
    }

    private void updateScoreText()
    {
        //scoreText.text = purpleScore.ToString() + "+" + blueScore.ToString();
        scoreText.text = new string($"Blue {blueScore.ToString()}  :  {purpleScore.ToString()} Purple");
    }

    public void ResetScene()
    {
        // End episode for both agents
        purpleAgent.EndEpisode();
        blueAgent.EndEpisode();

        purpleAgent.StopAllCoroutines();
        blueAgent.StopAllCoroutines();

        // Randomize agents' positions
        //float randomX = Random.Range(-25f, -3);
        //float randomZ = Random.Range(-14f, 14f);



        //GameObject heal1 = Instantiate(healPrefab, new Vector3(0f, transform.position.y, 15f), Quaternion.identity);
        //heal1.transform.parent = transform;
        //GameObject heal2 = Instantiate(healPrefab, new Vector3(0f, transform.position.y, -15f), Quaternion.identity);
        //heal2.transform.parent = transform;


        // Destroy all bullets
        var myBullets = transform.GetComponentsInChildren<Bullet>();
        foreach (Bullet bullet in myBullets)
        {
            Destroy(bullet.gameObject);
        }

        //Reset health for both agents
        purpleAgent.ResetHealth();
        blueAgent.ResetHealth();
    }

    private void increaseBlueScore()
    {
        blueScore += 1;
    }
    private void increasePurpleScore()
    {
        purpleScore += 1;
    }
}
