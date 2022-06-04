using System.Collections.Generic;
using UnityEngine;

public class MovableSpawner : MonoBehaviour
{
    [SerializeField] public float[] scrollSpeed;
    [SerializeField] public float[] distancesBetweenSpawns;
    [SerializeField] public float[] scrollSpeedMultipliers;
    [SerializeField] float[] spawnPositions;
    [SerializeField] GameObject[] obstacles;
    [SerializeField] float respawnThreshold = 1.5f;
    [SerializeField] float scoreBonusProbability = 0.3f;
    [SerializeField] public float invincibilityBonusProbability = 0.05f;
    [SerializeField] float horizontalBonusOffsetAmplitude = 1.75f;
    [SerializeField] float horizontalBonusOffsetAmplitudeInvincibile = 2f;
    [SerializeField] GameObject scoreBonus;
    [SerializeField] GameObject invincibilityBonus;
    [SerializeField] PlayerMovement player;
    [SerializeField] PowerManager powerManager;
    [SerializeField] PlayerOptions options;

    [HideInInspector] public bool active = false;
    [HideInInspector] public float normalScrollSpeed;
    [HideInInspector] public float invincibilityScrollSpeed;
    [HideInInspector] public float currentScrollSpeed;
    [HideInInspector] public float distanceBetweenSpawns;
    [HideInInspector] public float delayBetweenSpawnNormal;
    [HideInInspector] public float delayBetweenSpawnInvincible;
    [HideInInspector] public float currentDelayBetweenSpawn;
    [HideInInspector] public int numberOfSpawnedObstacles;
    float[,] holePositions;
    float[] lastHolePositions;
    float spawnPosition;
    float lastSpawnTime;
    Queue<GameObject> spawnedObstacles;
    Queue<GameObject> spawnedScoreBonuses;
    Queue<GameObject> spawnedInvincibilityBonuses;
    Queue<float> spawnTimesScoresBonus;
    int spawnedObstacleIndex;
    int lastSpawnedIndex = 0;
    float lastScoreBonus = 0f;
    float lastInvincibilityBonus = 0f;
    float lastMultipleScoreBonusSpawnTime = 0f;
    float currentScoreBonusProbability;
    float xOffset;
    float lastXOffset = 0f;
    bool hasScoreBonusSpawned;
    bool forcedScoreBonusSpawn = false;
    Vector3 bonusZOffsetPosition = Vector3.zero;
    Vector3 bonusXOffsetPosition = Vector3.zero;
    float scrollSpeedMultiplier;
    float lastAttemptedCleaningTime;
    float lastObstacleCleaningTime;
    float lastScoreBonusCleaningTime;
    float lastObstacleBonusCleaningTime;
    GameObject lastSpawnedObstacle;
    float obstaclePositionClosestToPlayer;
    float respawnDistance;

    void Awake()
    {
        spawnedObstacles = new Queue<GameObject>();
        spawnedScoreBonuses = new Queue<GameObject>();
        spawnedInvincibilityBonuses = new Queue<GameObject>();

        // Note the position of the hole for each obstacle
        holePositions = new float[obstacles.Length,2];
        for (int i=0; i<obstacles.Length; i++)
        {
            holePositions[i, 0] = obstacles[i].GetComponent<Holes>().positions[0];
            holePositions[i, 1] = obstacles[i].GetComponent<Holes>().positions[1];
        }
        lastHolePositions = new float[] {0f, 0f};
    }

    public void Initialize()
    {
        active = true;

        // Destroy spawned movables and clear queues
        ClearGameObjectQueue(ref spawnedObstacles);
        ClearGameObjectQueue(ref spawnedScoreBonuses);
        ClearGameObjectQueue(ref spawnedInvincibilityBonuses);

        // Set the appropriate difficulty
        // Obstacles positions
        spawnPosition = spawnPositions[options.difficulty];
        distanceBetweenSpawns = distancesBetweenSpawns[options.difficulty];
        respawnDistance = respawnThreshold * distanceBetweenSpawns;
        // Scroll speed
        normalScrollSpeed = scrollSpeed[options.difficulty];
        scrollSpeedMultiplier = scrollSpeedMultipliers[options.difficulty];
        invincibilityScrollSpeed = scrollSpeedMultiplier * normalScrollSpeed;
        // Delay between obstacles
        delayBetweenSpawnNormal = distanceBetweenSpawns / normalScrollSpeed;
        delayBetweenSpawnInvincible = distanceBetweenSpawns / invincibilityScrollSpeed;

        // Bonus offset position
        bonusZOffsetPosition = 0.5f * distanceBetweenSpawns * Vector3.forward;

        // Spawn two obstacles in advance
        numberOfSpawnedObstacles = 0;
        SpawnObjects(spawnPosition);
        SpawnObjects(spawnPosition + distanceBetweenSpawns);

        lastSpawnTime = Time.time;
    }

    // Previously Update: is it a problem?
    void FixedUpdate()
    {
        if (active)
        {
            // Update speed and spawn delay if player is invincible
            currentScrollSpeed = player.isInvincible ? invincibilityScrollSpeed : normalScrollSpeed;
            currentDelayBetweenSpawn = player.isInvincible ? delayBetweenSpawnInvincible : delayBetweenSpawnNormal;
            //currentDelayBetweenSpawn = delayBetweenSpawnNormal;

            // Spawn objects periodically
            if (lastSpawnedObstacle.transform.position.z - player.transform.position.z < respawnDistance)
            //if (Time.time - lastSpawnTime > currentDelayBetweenSpawn)
            {
                lastSpawnTime = Time.time;
                SpawnObjects(lastSpawnedObstacle.transform.position.z + distanceBetweenSpawns);
            }

            SpawnBonusesIfInvincible();

            // Periodically clean the queues of destroyed objects
            if (Time.time - lastAttemptedCleaningTime > currentDelayBetweenSpawn)
            {
                lastAttemptedCleaningTime = Time.time;
                CleanAllQueues();
            }
        }
    }

    private void SpawnBonusesIfInvincible()
    {
        if (player.isInvincible)
        {
            // Spawn bonuses for bonuses already spawned when invincibility started
            if (!forcedScoreBonusSpawn)
            {
                ObstaclePositionClosestToPlayer();
                float constantOffset = obstaclePositionClosestToPlayer - bonusZOffsetPosition.z;
                SpawnBonus(scoreBonus, 1f, constantOffset + (1 * 0.333333f) * distanceBetweenSpawns, ref spawnedScoreBonuses, ref lastScoreBonus);
                SpawnBonus(scoreBonus, 1f, constantOffset + (2 * 0.333333f) * distanceBetweenSpawns, ref spawnedScoreBonuses, ref lastScoreBonus);
                SpawnBonus(scoreBonus, 1f, constantOffset + (4 * 0.333333f) * distanceBetweenSpawns, ref spawnedScoreBonuses, ref lastScoreBonus);
                SpawnBonus(scoreBonus, 1f, constantOffset + (5 * 0.333333f) * distanceBetweenSpawns, ref spawnedScoreBonuses, ref lastScoreBonus);

                lastMultipleScoreBonusSpawnTime = Time.time;
                forcedScoreBonusSpawn = true;
            }

            // Spawn bonuses for new obstacles
            bool anticipatedEnd = Time.time - player.invincibilityStartTime < powerManager.invincibilityDuration - 1 * currentDelayBetweenSpawn;
            if (Time.time - lastMultipleScoreBonusSpawnTime > currentDelayBetweenSpawn && anticipatedEnd)
            {
                ObstaclePositionClosestToPlayer();
                float constantOffset = obstaclePositionClosestToPlayer - bonusZOffsetPosition.z;
                SpawnBonus(scoreBonus, 1f, constantOffset + (1 + 1 * 0.333333f) * distanceBetweenSpawns, ref spawnedScoreBonuses, ref lastScoreBonus);
                SpawnBonus(scoreBonus, 1f, constantOffset + (1 + 2 * 0.333333f) * distanceBetweenSpawns, ref spawnedScoreBonuses, ref lastScoreBonus);

                lastMultipleScoreBonusSpawnTime = Time.time;
            }
        }
        else
        {
            forcedScoreBonusSpawn = false;
        }
    }

    private void ObstaclePositionClosestToPlayer()
    {
        float minDistanceToPlayer = Mathf.Infinity;
        foreach (GameObject obstacle in spawnedObstacles)
        {
            if (obstacle == null)
                continue;

            float x = obstacle.transform.position.z - player.transform.position.z;
            if (x > 0 && obstacle.transform.position.z - player.transform.position.z < minDistanceToPlayer)
            {
                obstaclePositionClosestToPlayer = obstacle.transform.position.z;
                minDistanceToPlayer = obstaclePositionClosestToPlayer - player.transform.position.z;
            }
        }
    }

    // Spawn obstacles and bonuses
    void SpawnObjects(float position)
    {
        // OBSTACLES
        SpawnObstacle(position);

        // SCORE BONUSES (do not appear during invincibility because handled by a different method)
        bool continuousSpawnCondition = player.isInvincible && Time.time - player.invincibilityStartTime < powerManager.invincibilityDuration - 2 * delayBetweenSpawnInvincible;
        bool spawnAfterInvincibilityHasSpawned = Time.time - lastInvincibilityBonus < 4 * currentDelayBetweenSpawn;
        currentScoreBonusProbability = continuousSpawnCondition || spawnAfterInvincibilityHasSpawned ? 0f : scoreBonusProbability;
        hasScoreBonusSpawned = SpawnBonus(scoreBonus, currentScoreBonusProbability, position, ref spawnedScoreBonuses, ref lastScoreBonus);

        // INVINCIBILITY BONUS
        if (!hasScoreBonusSpawned && Time.time - lastInvincibilityBonus > (powerManager.numberOfBreakableObstacles + 1) * delayBetweenSpawnNormal)
        {
            SpawnBonus(invincibilityBonus, invincibilityBonusProbability, position, ref spawnedInvincibilityBonuses, ref lastInvincibilityBonus);
        }
    }

    private bool SpawnBonus(GameObject bonus, float probability, float position, ref Queue<GameObject> bonusQueue, ref float lastBonusTime)
    {
        bool spawned = Random.Range(0f, 1f) < probability;
        if (spawned)
        {
            // Ensure new bonus is horizontally shifted from previous bonus
            //do { xOffset = Random.Range(-3, 4) * horizontalBonusOffsetAmplitude; } while (Mathf.Abs(xOffset - lastXOffset) <= horizontalBonusOffsetAmplitude);
            if (player.isInvincible)
            {
                float sign = Random.Range(0, 2) * 2 - 1;
                xOffset = Mathf.Clamp(lastXOffset + sign * horizontalBonusOffsetAmplitudeInvincibile, -5, 5);
                if (xOffset == lastXOffset)
                    xOffset -= sign * horizontalBonusOffsetAmplitudeInvincibile;
            }
            else
            {
                do { xOffset = Random.Range(-3, 4) * horizontalBonusOffsetAmplitude; } while (Mathf.Abs(xOffset - lastXOffset) <= horizontalBonusOffsetAmplitude);
            }
            lastXOffset = xOffset;
            bonusXOffsetPosition = xOffset * Vector3.right;
            GameObject newBonus = Instantiate(bonus, transform.position + position * Vector3.forward + bonusZOffsetPosition + bonusXOffsetPosition, Quaternion.identity);
            bonusQueue.Enqueue(newBonus);
            lastBonusTime = Time.time;
        }
        return spawned;
    }

    private void SpawnObstacle(float position)
    {
        spawnedObstacleIndex = Random.Range(0, obstacles.Length);
        // Ensure new obstacle is different from previous obstacle
        do {spawnedObstacleIndex = Random.Range(0, obstacles.Length);} while (!HasAdaptedHoles(spawnedObstacleIndex));
        lastSpawnedIndex = spawnedObstacleIndex;
        lastHolePositions[0] = holePositions[spawnedObstacleIndex, 0];
        lastHolePositions[1] = holePositions[spawnedObstacleIndex, 1];
        GameObject newObstacle = Instantiate(obstacles[spawnedObstacleIndex], transform.position + position * Vector3.forward, Quaternion.identity);
        spawnedObstacles.Enqueue(newObstacle);

        lastSpawnedObstacle = newObstacle;
        numberOfSpawnedObstacles++;
    }

    private void CleanAllQueues()
    {
        CleanGameObjectQueue(ref spawnedObstacles, ref lastObstacleCleaningTime);
        CleanGameObjectQueue(ref spawnedScoreBonuses, ref lastScoreBonusCleaningTime);
        CleanGameObjectQueue(ref spawnedInvincibilityBonuses, ref lastObstacleBonusCleaningTime);
    }

    void CleanGameObjectQueue(ref Queue<GameObject> queue, ref float lastCleaningTime)
    {
        if (queue.Count > 1 && queue.Peek() == null)
        {
            queue.Dequeue();
            lastCleaningTime = Time.time;
        }
    }

    void ClearGameObjectQueue(ref Queue<GameObject> queue)
    {
        foreach (GameObject obj in queue)
        {
            Destroy(obj);
        }
        queue.Clear();
    }

    private bool HasAdaptedHoles(int obstacleIndex)
    {
        for (int i=0; i<2; i++)
        {
            float threshold = 2f;
            if (Mathf.Abs(holePositions[obstacleIndex, i] - lastHolePositions[0]) <= threshold || Mathf.Abs(holePositions[obstacleIndex, i] - lastHolePositions[1]) <= threshold)
                return false;
        }
        return true;
    }
}
