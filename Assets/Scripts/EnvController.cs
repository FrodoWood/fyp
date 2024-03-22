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
            purpleAgent.AddReward(2f);
            blueAgent.AddReward(-2f);
            increasePurpleScore();
            ResetScene();
            return;
        }
        else if (purpleAgent.currentState == State.Dead)
        {
            blueAgent.AddReward(2f);
            purpleAgent.AddReward(-2f);
            increaseBlueScore();
            ResetScene();
            return;
        }

        if(purpleAgent.isDq || blueAgent.isDq)
        {
            ResetScene();
            return;
        }
        
        if(purpleAgent.StepCount >= purpleAgent.MaxStep -1 || blueAgent.StepCount >= blueAgent.MaxStep-1)
        {
            ResetScene();
            return;
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

        purpleAgent.ResetCooldown();
        blueAgent.ResetCooldown();

        purpleAgent.StopAllCoroutines();
        blueAgent.StopAllCoroutines();

        purpleAgent.isDq = false;
        blueAgent.isDq = false;

        float randomX = Random.Range(-30,-9);
        float randomZ = Random.Range(-10,10);

        float prob = Random.value;
        if(prob < 0.5)
        {
            purpleAgent.transform.position = new Vector3(randomX, purpleAgent.transform.position.y, randomZ);
            blueAgent.transform.position = new Vector3(-randomX, purpleAgent.transform.position.y, -randomZ);
        }
        else
        {
            purpleAgent.transform.position = new Vector3(-randomX, purpleAgent.transform.position.y, -randomZ);
            blueAgent.transform.position = new Vector3(randomX, purpleAgent.transform.position.y, randomZ);
        }

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
