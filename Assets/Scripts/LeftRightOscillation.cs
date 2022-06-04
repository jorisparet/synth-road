using UnityEngine;

public class LeftRightOscillation : MonoBehaviour
{
    [SerializeField] float frequency = 1f;
    [SerializeField] float bound = 1f;

    float X;
    float phaseShift;

    private void Awake()
    {
        phaseShift = Random.Range(0f, Mathf.PI);
        ChangePosition();
    }

    void Update()
    {
        ChangePosition();
    }

    void ChangePosition()
    {
        X = bound * Mathf.Sin(frequency * Time.time + phaseShift);
        transform.position = new Vector3(X, transform.position.y, transform.position.z);
    }
}
