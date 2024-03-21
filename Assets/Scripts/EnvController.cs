using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnvController : MonoBehaviour
{
    [SerializeField] private EnemyController purpleAgent;
    [SerializeField] private EnemyController blueAgent;
    [SerializeField] private Transform purpleGoal;
    [SerializeField] private Transform blueGoal;
    [SerializeField] private Heal heal1;
    [SerializeField] private Heal heal2;

    public int purpleScore;
    public int blueScore;
    public TextMeshProUGUI scoreText;
    
    

    private void Update()
    {
        updateScoreText();

        if (purpleAgent.hasWon)
        {
            purpleAgent.AddReward(4f);
            blueAgent.AddReward(-2f);
            increasePurpleScore();
            ResetScene();
        }
        else if (blueAgent.hasWon)
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
        
        if(purpleAgent.currentState == State.Dead && blueAgent.currentState == State.Dead)
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

        float randomX = -25f;
        float randomZ = 0f; ;

        float prob = Random.value;
        if(prob < 0.5)
        {
            purpleAgent.transform.position = new Vector3(randomX, purpleAgent.transform.position.y, randomZ);
            purpleGoal.position = new Vector3(32, transform.position.y, 0);
            blueAgent.transform.position = new Vector3(-randomX, purpleAgent.transform.position.y, -randomZ);
            blueGoal.position = new Vector3(-32, transform.position.y, 0);
        }
        else
        {
            purpleAgent.transform.position = new Vector3(-randomX, purpleAgent.transform.position.y, -randomZ);
            purpleGoal.position = new Vector3(-32, transform.position.y, 0);
            blueAgent.transform.position = new Vector3(randomX, purpleAgent.transform.position.y, randomZ);
            blueGoal.position = new Vector3(32, transform.position.y, 0);
        }

        //GameObject heal1 = Instantiate(healPrefab, new Vector3(0f, transform.position.y, 15f), Quaternion.identity);
        //heal1.transform.parent = transform;
        //GameObject heal2 = Instantiate(healPrefab, new Vector3(0f, transform.position.y, -15f), Quaternion.identity);
        //heal2.transform.parent = transform;

        // Reset heals
        heal1.Activate();
        heal2.Activate();

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
