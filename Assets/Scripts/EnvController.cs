using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvController : MonoBehaviour
{
    [SerializeField] private EnemyController purpleAgent;
    [SerializeField] private EnemyController blueAgent;
    

    private void Update()
    {
        if(purpleAgent.currentState == State.Dead || blueAgent.currentState == State.Dead)
        {
            
            ResetScene();
        }
        
    }

    public void ResetScene()
    {
        // End episode for both agents
        purpleAgent.EndEpisode();
        blueAgent.EndEpisode();

        // Randomize agents' positions
        float randomX = Random.Range(-14f, 14f);
        float randomZ = Random.Range(-14f, 14f);

        purpleAgent.transform.position = new Vector3(randomX, purpleAgent.transform.position.y, randomZ);
        blueAgent.transform.position = new Vector3(-randomX, purpleAgent.transform.position.y, -randomZ);

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


}
