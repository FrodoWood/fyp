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

        if (purpleAgent.hasWon)
        {
            purpleAgent.AddReward(2f);
            blueAgent.AddReward(-2f);
            increasePurpleScore();
            ResetScene();
        }
        else if (blueAgent.hasWon)
        {
            blueAgent.AddReward(2f);
            purpleAgent.AddReward(-2f);
            increaseBlueScore();
            ResetScene();
        }

        if(purpleAgent.StepCount >= purpleAgent.MaxStep -1 || blueAgent.StepCount >= blueAgent.MaxStep-1)
        {
            //purpleAgent.SetReward(0f);
            //blueAgent.SetReward(0f);
            ResetScene();
            return;
        }
        
        if(purpleAgent.currentState == State.Dead && blueAgent.currentState == State.Dead)
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
