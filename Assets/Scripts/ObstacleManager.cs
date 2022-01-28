using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [SerializeField] public float scrollSpeed = 20f;
    [SerializeField] float delayBetweenSpawn = 2f;
    [SerializeField] float spawnPosition = 50f;
    [SerializeField] GameObject[] obstacles;
    [SerializeField] float bonusSpawnProbability = 0.05f;
    [SerializeField] GameObject bonus;

    bool active = false;
    float lastSpawnTime;
    List<GameObject> spawnedObstacles;
    List<GameObject> spawnedBonuses;
    int spawnedObstacleIndex;
    int lastSpawnedIndex = 0;
    Vector3 bonusOffsetPosition = 7.5f * Vector3.forward;

    // Start is called before the first frame update
    void Awake()
    {
        lastSpawnTime = 0f;
        spawnedObstacles = new List<GameObject>();
        spawnedBonuses = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        SpawnObjects();
        DestroySpawnedObjects(ref spawnedObstacles);
        DestroySpawnedObjects(ref spawnedBonuses);
    }

    void SpawnObjects()
    {
        if (Time.time - lastSpawnTime > delayBetweenSpawn && active)
        {
            lastSpawnTime = Time.time;
            spawnedObstacleIndex = Random.Range(0, obstacles.Length);
            // Ensure new obstacle each time
            do
            {
                spawnedObstacleIndex = Random.Range(0, obstacles.Length);
            }
            while (spawnedObstacleIndex == lastSpawnedIndex);
            lastSpawnedIndex = spawnedObstacleIndex;

            GameObject newObstacle = Instantiate(obstacles[spawnedObstacleIndex], transform.position + Vector3.forward * spawnPosition, Quaternion.identity);
            newObstacle.GetComponent<Rigidbody>().velocity = Vector3.back * scrollSpeed;
            spawnedObstacles.Add(newObstacle);

            // Bonuses
            if (Random.Range(0f, 1f) < bonusSpawnProbability)
            {
                GameObject newBonus = Instantiate(bonus, transform.position + Vector3.forward * spawnPosition + bonusOffsetPosition, Quaternion.identity);
                newBonus.GetComponent<Rigidbody>().velocity = Vector3.back * scrollSpeed;
                spawnedBonuses.Add(newBonus);
            }
        }
    }

    void DestroySpawnedObjects(ref List<GameObject> objectList)
    {
        if (objectList.Count > 1)
        {
            GameObject oldestObject = objectList[0];
            if (oldestObject.transform.position.z < -5f)
            {
                objectList.Remove(oldestObject);
                Destroy(oldestObject);
            }
        }
    }

    public void Initialize()
    {
        active = true;

        foreach (GameObject obj in spawnedBonuses)
        {
            Destroy(obj);
        }
        spawnedBonuses.Clear();

        foreach (GameObject obj in spawnedObstacles)
        {
            Destroy(obj);
        }
        spawnedObstacles.Clear();
    }
}
