using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public PathFollower enemyPrefab;
    public Transform[] waypoints;
    public float interval = 1f;
    public int count = 5;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        for (int i = 0; i < count; i++)
        {
            PathFollower enemy = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);
            enemy.waypoints = waypoints;

            yield return new WaitForSeconds(interval);
        }
    }


}
