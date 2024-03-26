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

    public bool shootingTraining = false;

    private void Start()
    {
        ResetScene();
    }

    private void Update()
    {
        updateScoreText();

        float purpleAgentCumulativeReward = purpleAgent.GetCumulativeReward();
        float blueAgentCumulativeReward = blueAgent.GetCumulativeReward();

        if (purpleAgent.hasWon)
        {
            purpleAgent.AddReward(purpleAgentCumulativeReward);
            blueAgent.AddReward(-2f);
            increasePurpleScore();
            ResetScene();
            return;
        }
        else if (blueAgent.hasWon)
        {
            blueAgent.AddReward(blueAgentCumulativeReward);
            purpleAgent.AddReward(-2f);
            increaseBlueScore();
            ResetScene();
            return;
        }

        if(purpleAgent.StepCount >= purpleAgent.MaxStep -1 || blueAgent.StepCount >= blueAgent.MaxStep-1)
        {
            //purpleAgent.SetReward(0f);
            //blueAgent.SetReward(0f);
            ResetScene();
            return;
        }
        
        if(!purpleAgent.isAlive && !blueAgent.isAlive)
        {
            //purpleAgent.SetReward(0f);
            //blueAgent.SetReward(0f);
            ResetScene();
            return;
        }

        //Shooting training
        if (shootingTraining)
        {
            if(!purpleAgent.isAlive || !blueAgent.isAlive)
            {
                ResetScene();
                return;
            }
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
        
        // Destroy all bullets
        var myBullets = transform.GetComponentsInChildren<Bullet>();
        foreach (Bullet bullet in myBullets)
        {
            Destroy(bullet.gameObject);
        }

        //Reset health for both agents
        purpleAgent.ResetHealth();
        blueAgent.ResetHealth();

        // Reset heals
        heal1.Activate();
        heal2.Activate();

        // Shooting training -------
        // Reset pos
        if (shootingTraining)
        {
            blueAgent.transform.position = new Vector3(0, transform.position.y, 0);
            purpleAgent.transform.position = GetRandomPositionInCircle(Random.Range(20,25));
            return;
        }

        //Normal training -------
        

        // Reset pos
        float prob = Random.value;
        float posX;
        float posZ;
        if(prob < 0.5)
        {
            posX = 25;
            posZ = 25;
        }
        else
        {
            posX = -25;
            posZ = -25;
        }

        purpleAgent.transform.position = new Vector3(posX, purpleAgent.transform.position.y, posZ);
        purpleGoal.position = new Vector3(-posX, transform.position.y, -posX);

        blueAgent.transform.position = new Vector3(-posX, purpleAgent.transform.position.y, -posZ);
        blueGoal.position = new Vector3(posX, transform.position.y, posX);
    }

    private void increaseBlueScore()
    {
        blueScore += 1;
    }
    private void increasePurpleScore()
    {
        purpleScore += 1;
    }

    Vector3 GetRandomPositionInCircle(float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        float y = transform.position.y;
        return new Vector3(x, y, z);
    }
}
