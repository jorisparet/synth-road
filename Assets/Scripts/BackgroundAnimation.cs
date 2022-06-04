using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAnimation : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] float angleIncrement = 0.1f;
    [SerializeField] float activeYPositionThreshold = -10f;
    [SerializeField] GameObject[] celestialBodies;
    [Header("Clouds")]
    [SerializeField] Renderer cloudRenderer;
    [SerializeField] float topIntensity = 3.5f;
    [SerializeField] float bottomIntensity = 5f;
    [SerializeField] Color baseTopColor;
    [SerializeField] Color baseBottomColor;
    [SerializeField] Color targetTopColor;
    [SerializeField] Color targetBottomColor;
    [Header("Mountains")]
    [SerializeField] Material mountainMaterial;
    [SerializeField] float sunColorIntensity = 10f;
    [SerializeField] float moonColorIntensity = 10f;
    [SerializeField] Color mountainSunColor;
    [SerializeField] Color mountainMoonColor;

    Color scaledBaseTopColor;
    Color scaledBaseBottomColor;
    Color scaledTargetTopColor;
    Color scaledTargetBottomColor;
    Color scaledMountainSunColor;
    Color scaledMountainMoonColor;

    string _CloudTopColor = "_TopColor";
    string _CloudBottomColor = "_BottomColor";
    string _MountainShineColor = "_ShineColor";

    float period;

    private void Awake()
    {
        scaledBaseTopColor = topIntensity * baseTopColor;
        scaledBaseBottomColor = bottomIntensity * baseBottomColor;
        scaledTargetTopColor = topIntensity * targetTopColor;
        scaledTargetBottomColor = bottomIntensity * targetBottomColor;
        scaledMountainSunColor = sunColorIntensity * mountainSunColor;
        scaledMountainMoonColor = moonColorIntensity * mountainMoonColor;
    }

    void Start()
    {
        cloudRenderer.sharedMaterial.SetColor(_CloudTopColor, scaledBaseTopColor);
        cloudRenderer.sharedMaterial.SetColor(_CloudBottomColor, scaledBaseBottomColor);
        mountainMaterial.SetColor(_MountainShineColor, scaledMountainSunColor);
        period = 360f / angleIncrement * Time.fixedDeltaTime;
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float t = 0;
        float elapsed = 0f;
        float constCos = 2 * Mathf.PI / period;
        while (true)
        {
            t = 1 - (Mathf.Cos(constCos * elapsed) / 2 + 0.5f);

            transform.Rotate(-Vector3.forward, angleIncrement);
            foreach (GameObject celestialBody in celestialBodies)
            {
                if (celestialBody.transform.position.y < activeYPositionThreshold)
                    celestialBody.SetActive(false);
                else if (celestialBody.transform.position.y > activeYPositionThreshold && !celestialBody.activeSelf)
                    celestialBody.SetActive(true);

                celestialBody.transform.Rotate(Vector3.down, angleIncrement);
            }
            cloudRenderer.sharedMaterial.SetColor(_CloudTopColor, Color.Lerp(scaledBaseTopColor, scaledTargetTopColor, t));
            cloudRenderer.sharedMaterial.SetColor(_CloudBottomColor, Color.Lerp(scaledBaseBottomColor, scaledTargetBottomColor, t));
            mountainMaterial.SetColor(_MountainShineColor, Color.Lerp(scaledMountainSunColor, scaledMountainMoonColor, t));

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }
}
