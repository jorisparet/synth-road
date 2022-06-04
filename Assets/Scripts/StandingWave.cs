using UnityEngine;

public class StandingWave : MonoBehaviour
{
    [SerializeField] float frequency = 1f;
    [SerializeField] float bound = 2.5f;
    [SerializeField] int mode = 2;
    [SerializeField] float verticalShift = 0.75f;
    [SerializeField] float amplitude = 1f;
    [SerializeField] GameObject[] walls;

    float lambda;
    float halfBound;
    float Y;
    float waveConstant;

    private void Start()
    {
        lambda = 4 * bound / mode;
        halfBound = bound / 2;
        Y = walls[0].transform.position.y;
        waveConstant = 2 * Mathf.PI / lambda;

        AnimateWave();
    }

    private void Update()
    {
        AnimateWave();
    }

    private void AnimateWave()
    {
        int wallIndex = 1;
        foreach (GameObject wall in walls)
        {
            Y = verticalShift + amplitude * Mathf.Sin(waveConstant * (wall.transform.position.x + halfBound)) * Mathf.Cos(frequency * Time.time);
            wall.transform.position = new Vector3(wall.transform.position.x, Y, wall.transform.position.z);
            wallIndex++;
        }
    }
}
