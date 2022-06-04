using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ForceField : Power
{
    [Header("Power-specific Settings")]

    // Main features
    [SerializeField] float rescalingSpeed = 0.75f;
    [SerializeField] float originalScale = 0.9f;
    [SerializeField] GameObject forceField;
    [SerializeField] bool unlock = false;
    // Sound effects
    [SerializeField] AudioClip forceFieldSound;

    Material forceFieldMaterial;

    protected override void Awake()
    {
        base.Awake();
        unlocked = unlock;
        forceFieldMaterial = forceField.GetComponent<Renderer>().sharedMaterial;
    }

    public override void Initialize()
    {
        base.Initialize();

        forceField.SetActive(false);
        forceField.transform.localScale = originalScale * Vector3.one;
    }

    protected override void StartPower()
    {
        forceField.SetActive(true);
        audioSource.PlayOneShot(forceFieldSound);
        StartCoroutine(RescaleForceField(duration));
    }

    protected override void EndPower()
    {
        forceField.SetActive(false);
    }

    //private void EnableForceField()
    //{
    //    if (!activated && isPressed && (Time.unscaledTime - lastActivation > reloadTime + duration) && powerManager.fullBar)
    //    {
    //        activated = true;
    //        forceField.SetActive(true);
    //        lastActivation = Time.unscaledTime;
    //        audioSource.PlayOneShot(forceFieldSound);
    //        StartCoroutine(RescaleForceField(duration));
    //        powerManager.EmptyBar(duration);
    //    }
    //    else if (activated && (Time.unscaledTime - lastActivation > duration))
    //    {
    //        activated = false;
    //        forceField.SetActive(false);
    //        isPressed = false;
    //        powerManager.ReloadBar(reloadTime);
    //    }
    //}

    IEnumerator RescaleForceField(float duration)
    {
        float elapsed = 0f;
        float t;
        while (elapsed < duration)
        {
            t = Mathf.Clamp(1f - ((duration - elapsed) / duration), 0, 1);
            forceFieldMaterial.SetFloat("_Alpha", Mathf.Lerp(1,0,t));
            forceField.transform.localScale += rescalingSpeed * Vector3.one * Time.deltaTime;
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
        forceField.transform.localScale = originalScale * Vector3.one;
        forceFieldMaterial.SetFloat("_Alpha", 1f);
    }
}
