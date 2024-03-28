using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class EnvController : MonoBehaviour
{
    [SerializeField] private EnemyController purpleAgent;
    [SerializeField] private EnemyController blueAgent;
    [SerializeField] private Transform purpleGoal;
    [SerializeField] private Transform blueGoal;
    public float timerInSeconds;
    private float timeToGoal;
    public int totalSteps;

    public int purpleScore;
    public int blueScore;
    public TextMeshProUGUI scoreText;

    public string winner = "Draw";
    const string blue = "Blue";
    const string purple = "Purple";

    public bool shootingTraining = false;

    public int numberRounds = 10;
    private int currentRound = 0;
    private List<string[]> dataRows = new List<string[]>();

    EnemyController[] enemyControllers;

    private void Start()
    {
        ResetTimer();
        //ResetScene();
        Setup();
        
    }


    private void FixedUpdate()
    {
        timerInSeconds -= Time.fixedDeltaTime;
        // Update UI TODO
    }

    private void Update()
    {
        updateScoreText();

        float purpleAgentCumulativeReward = purpleAgent.GetCumulativeReward();
        float blueAgentCumulativeReward = blueAgent.GetCumulativeReward();

        if (purpleAgent.hasWon)
        {
            winner = purple;
            purpleAgent.AddReward(Mathf.Abs(purpleAgentCumulativeReward) +1f);
            blueAgent.AddReward(-Mathf.Abs(blueAgentCumulativeReward) -1f);
            increasePurpleScore();
            purpleAgent?.AddScore(purpleAgent.score);
            timeToGoal = (totalSteps * Time.fixedDeltaTime) - timerInSeconds;
            ResetScene();
            return;
        }
        else if (blueAgent.hasWon)
        {
            winner = blue;
            blueAgent.AddReward(Mathf.Abs(blueAgentCumulativeReward) +1f);
            purpleAgent.AddReward(-Mathf.Abs(purpleAgentCumulativeReward) -1f);
            increaseBlueScore();
            blueAgent?.AddScore(blueAgent.score);
            timeToGoal = (totalSteps * Time.fixedDeltaTime) - timerInSeconds;
            ResetScene();
            return;
        }

        if(purpleAgent.StepCount >= purpleAgent.MaxStep -1 || blueAgent.StepCount >= blueAgent.MaxStep-1)
        {
            ResetScene();
            return;
        }
        
        if(!purpleAgent.isAlive && !blueAgent.isAlive)
        {
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

    public void Setup()
    {
        // Reset individual scores
        blueAgent.score = 0;
        purpleAgent.score = 0;
        // Reset timer
        ResetTimer();
        timeToGoal = 0;
        // Reset winner
        winner = "Draw";
        float prob = Random.value;
        float posX;
        float posZ;
        if (prob < 0.5)
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

    public void ResetScene()
    {

        // Export data TODO
        Debug.Log($"\nWinner: {winner}");
        Debug.Log($"Blue score: {blueAgent.score}");
        Debug.Log($"Purple score: {purpleAgent.score}");
        Debug.Log($"Blue: {(blueAgent.isAlive? "Alive" : "Dead")}");
        Debug.Log($"Purple: {(purpleAgent.isAlive? "Alive" : "Dead")}");
        Debug.Log($"Time to goal: {(timeToGoal == 0? "" : timeToGoal)}");
        Debug.Log($"Time left: {timerInSeconds}");

        SaveRoundData(winner, blueAgent.score, purpleAgent.score, blueAgent.isAlive, purpleAgent.isAlive, timeToGoal, timerInSeconds);
        currentRound += 1;

        if(currentRound >= numberRounds)
        {
            ExportToCSV();
            gameObject.SetActive(false);
            return;
        }
        
        // Reset individual scores
        blueAgent.score = 0;
        purpleAgent.score = 0;
        // Reset timer
        ResetTimer();
        timeToGoal = 0;
        // Reset winner
        winner = "Draw";
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
        // Destroy all bullets
        var heals = transform.GetComponentsInChildren<Heal>(true);
        foreach (Heal heal in heals)
        {
            heal.Activate();
        }

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

    private void SaveRoundData(string winner, int blueScore, int purpleScore, bool blueAlive, bool purpleAlive, float timeToGoal, float timeLeft)
    {
        string[] rowData = new string[]
        {
            winner,
            blueScore.ToString(),
            purpleScore.ToString(),
            blueAlive ? "Alive" : "Dead",
            purpleAlive ? "Alive" : "Dead",
            (timeToGoal == 0) ? "" : timeToGoal.ToString(),
            timerInSeconds.ToString()
        };

        dataRows.Add(rowData);
    }

    private void ExportToCSV()
    {
        string directoryPath = Application.dataPath + "/DataExport";
        string filePath = directoryPath + "/test.csv";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if(!File.Exists(filePath))
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("Winner, Blue Score, Purple Score, Blue State, Purple State, Time to Goal, Time Left");
            }
        }

        using(StreamWriter sw = File.AppendText(filePath))
        {
            foreach (string[] rowData in dataRows)
            {
                sw.WriteLine(string.Join(",", rowData));
            }
        }

        Debug.Log("Data exported to: " + filePath);
    }

    private void ResetTimer()
    {
        timerInSeconds = totalSteps * Time.fixedDeltaTime;
    }
}
