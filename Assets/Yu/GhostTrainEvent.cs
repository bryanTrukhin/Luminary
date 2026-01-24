using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class GhostTrainEvent : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private bool oneShot = true;

    [Header("Screen Effect")]
    [SerializeField] private Image redOverlay;            // GhostTrainOverlay
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.75f;
    [SerializeField, Min(0.1f)] private float timeToMax = 3f;
    [SerializeField] private AnimationCurve intensityCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Lantern Reaction")]
    [SerializeField] private float flickerAmplitude = 0.8f;
    [SerializeField] private float flickerSpeed     = 5f;

    [Header("Optional FX")]
    [SerializeField] private AudioSource rumbleAudio;
    [SerializeField] private ParticleSystem smokeTrail;

    private bool _running;
    private bool _used;
    private PlayerController _player;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (_running) return;
        if (_used && oneShot) return;
        if (!col.CompareTag("Player")) return;

        _player = col.GetComponentInParent<PlayerController>();
        if (_player == null) return;

        StartCoroutine(RunTrainRoutine());
    }

    IEnumerator RunTrainRoutine()
    {
        _running = true;
        _used = true;

        // Ensure overlay starts fully transparent
        if (redOverlay != null)
        {
            var c = redOverlay.color;
            c.a = 0f;
            redOverlay.color = c;
        }

        // Start FX
        if (rumbleAudio != null) rumbleAudio.Play();
        if (smokeTrail != null) smokeTrail.Play();

        if (_player != null && _player.Lantern != null)
        {
            _player.Lantern.Flicker(
                flickerAmplitude,
                flickerSpeed,
                timeToMax
            );
        }

        float t = 0f;

        while (t < timeToMax)
        {
            // If we hid in a locker, cancel and fade out
            if (GameController.I != null && GameController.I.PlayerSafeFromMonster())
            {
                yield return FadeOutOverlay(0.3f);
                StopEffects();
                _running = false;
                yield break;
            }

            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / timeToMax);
            float curve      = intensityCurve.Evaluate(normalized);

            if (redOverlay != null)
            {
                var c = redOverlay.color;
                c.a = Mathf.Lerp(0f, maxAlpha, curve);   // <- progressive fade
                redOverlay.color = c;
            }

            yield return null;
        }

        // Hit max danger & not hiding → death
        if (GameController.I != null &&
            _player != null &&
            !GameController.I.PlayerSafeFromMonster())
        {
            GameController.I.KillPlayer(_player);
        }

        _running = false;
    }

    IEnumerator FadeOutOverlay(float duration)
    {
        if (redOverlay == null) yield break;

        float t = 0f;
        Color c = redOverlay.color;
        float startA = c.a;

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = t / duration;
            c.a = Mathf.Lerp(startA, 0f, u);
            redOverlay.color = c;
            yield return null;
        }

        c.a = 0f;
        redOverlay.color = c;
    }

    void StopEffects()
    {
        if (rumbleAudio != null) rumbleAudio.Stop();
        if (smokeTrail != null) smokeTrail.Stop();
    }
}