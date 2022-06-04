using UnityEngine;

public class ProgressiveWave : MonoBehaviour
{
    [SerializeField] float frequency = 1f;
    [SerializeField] float bound = 2.5f;
    [SerializeField] float phaseShiftDivider = 10f;
    [SerializeField] GameObject[] walls;

    int sign;
    float Y_0;
    float Y;
    float phaseShift;

    private void Start()
    {
        sign = Random.Range(0, 2) * 2 - 1; // left or right propagation
        Y_0 = walls[0].transform.position.y;
        phaseShift = Mathf.PI / phaseShiftDivider;

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
            Y = Y_0 - bound * Mathf.Abs(Mathf.Sin(sign * frequency * Time.time + wallIndex * phaseShift));
            wall.transform.position = new Vector3(wall.transform.position.x, Y, wall.transform.position.z);
            wallIndex++;
        }
    }
}
