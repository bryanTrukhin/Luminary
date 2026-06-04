using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    public static DamageFlash I;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerDamageFlash()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Volume globalVolume = FindObjectOfType<Volume>();

        if (globalVolume != null && globalVolume.profile.TryGet(out ColorAdjustments ca))
        {
            ca.colorFilter.value = Color.red;
            yield return new WaitForSecondsRealtime(0.25f);
            ca.colorFilter.value = Color.white;
        }
        else
        {
            Debug.LogWarning("DamageFlash: Could not find Volume or ColorAdjustments in this scene!");
        }
    }
}