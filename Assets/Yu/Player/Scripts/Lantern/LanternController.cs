using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

[DisallowMultipleComponent]
public class LanternController : MonoBehaviour, ILantern
{

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flashSound;
    [SerializeField] private AudioClip dashSound;
    [Header("Firefly")]
    [SerializeField] private float fireflyCapacity;
    [SerializeField] private float fireflyRegenRate;
    public float _currentFireflies;
    public TMP_Text currentFireFlyCountUI;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 1.0f; // I-Frames
    private float _damageTimer;

    [Header("Movement Abilities")]
    [SerializeField] private float dashCost;
    [SerializeField] private float doubleJumpCost;

    [Header("Abilities")]
    [SerializeField] private float flashLanternCost;
    [SerializeField] private float flashBonusIntensity;
    [SerializeField] private float flashDuration;
    [SerializeField] private float flashCooldown;
    private float _flashCooldownTimer;

    [Header("Burning Mechanics")]
    [SerializeField] private LayerMask burnableMask;
    [SerializeField] private float burnRadius = 1.5f;
    [SerializeField] private float burnDamagePerTick = 1f;
    [SerializeField] private float burnTickInterval = 0.1f;

    [Header("Visuals")]
    [SerializeField] private List<Light2D> bulbs = new();
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Vector3 rightRest = new Vector3(0.55f, -0.20f, 0);
    [SerializeField] private Vector3 leftRest = new Vector3(-0.55f, -0.20f, 0);
    [SerializeField] private float smoothTime = 0.08f;


    // Internal State
    struct BulbState { public float intensity; public float radius; }
    private readonly List<BulbState> _originals = new();
    private Vector3 _swingVelocity;
    private Coroutine _flickerCo;


    void Start()
    {
        ResetHealth();
    }

    void Awake()
    {
        // Auto-setup lights and root
        if (bulbs.Count == 0) bulbs.AddRange(GetComponentsInChildren<Light2D>(includeInactive: true));
        if (playerRoot == null) playerRoot = transform.parent;

        foreach (var b in bulbs)
            _originals.Add(new BulbState { intensity = b.intensity, radius = b.pointLightOuterRadius });
        
        // Initial position
        bool facingRight = playerRoot.localScale.x >= 0f;
        transform.localPosition = facingRight ? rightRest : leftRest;
    }

    void Update()
    {
        //new: display num of fireflies
        currentFireFlyCountUI.text = _currentFireflies.ToString("0.0") + "/" + fireflyCapacity;
        
        // 1. Handle Cooldowns internally
        if (_flashCooldownTimer > 0) _flashCooldownTimer -= Time.deltaTime;

        // 2. Regenerate Fireflies
        if (_currentFireflies < fireflyCapacity && fireflyRegenRate > 0f)
        {
            _currentFireflies += fireflyRegenRate * Time.deltaTime;
            _currentFireflies = Mathf.Min(_currentFireflies, fireflyCapacity);
        }
        
        if (InputSystem.FlashLantern())
        {
            TriggerFlash();
        }
    }
    public bool TryConsumeEnergy(float amount)
    {
        if (_currentFireflies >= amount)
        {
            _currentFireflies -= amount;
            return true;
        }
        
        Debug.Log("Not enough Fireflies!"); 
        return false;
    }
    public void TakeDamage(float amount)
    {
        //Check for I-Frames or God Mode
        if (_damageTimer > 0 || GameController.I.State == PlayState.Dead) return;

        _currentFireflies -= amount;
        // Visual Feedback
        Flicker(0.5f, 20f, 0.5f); 
        _damageTimer = damageCooldown;

        // Check Death
        if (_currentFireflies <= 0)
        {
            _currentFireflies = 0;
            PlayerController pc = playerRoot.GetComponent<PlayerController>();
            GameController.I.KillPlayer(pc);
        }
    }
    public void ResetHealth()
    {
        _currentFireflies = fireflyCapacity;
        _damageTimer = 0;
    }

    // ================= Lantern Abilities ================= //

    public void TriggerFlash()
    {
        // Check Cooldown AND Firefly count
        bool result = TryConsumeEnergy(flashLanternCost);
        if (result && flashSound)
        {
            audioSource.clip = flashSound;
            audioSource.Play();
        }
        if (_flashCooldownTimer <= 0f && TryConsumeEnergy(flashLanternCost))
        {
            Flash(flashBonusIntensity, flashDuration);
            _flashCooldownTimer = flashCooldown;
        }
    }

    public bool TryUseDash()
    {
        bool result = TryConsumeEnergy(dashCost);
        if (result && dashSound)
        {
            audioSource.clip = dashSound;
            audioSource.Play();
        }
        return result;
    }

    public bool TryUseDoubleJump()
    {
        bool result = TryConsumeEnergy(doubleJumpCost);
        return result;
    }




    // ================= Lantern Visuals and COROUTINES ================= //
    void LateUpdate()
    {
        // Handle Swing Logic
        bool facingRight = playerRoot.localScale.x >= 0f;
        Vector3 targetOffset = facingRight ? rightRest : leftRest;
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetOffset, ref _swingVelocity, smoothTime);
    }

    public void SetVisible(bool on)
    {
        foreach (var b in bulbs) b.enabled = on;
        
        var sr = GetComponent<SpriteRenderer>();
        if(sr) sr.enabled = on;
    }
    public void Flicker(float amplitude, float speed, float duration)
    {
        if (_flickerCo != null) StopCoroutine(_flickerCo);
        _flickerCo = StartCoroutine(FlickerRoutine(amplitude, speed, duration));
    }

    private void Flash(float bonusIntensity, float duration)
    {
        if (_flickerCo != null) StopCoroutine(_flickerCo);
        if (flashSound)
        {
            audioSource.clip = flashSound;
            audioSource.Play();
        }
        _flickerCo = StartCoroutine(FlashRoutine(bonusIntensity, duration));
    }

    IEnumerator FlashRoutine(float bonusI, float dur)
    {
        float endTime = Time.time + dur;
        float nextTickTime = Time.time; 
        
        while (Time.time < endTime)
        {
            if (Time.time >= nextTickTime)
            {
                ApplyBurnTick(); 
                nextTickTime += burnTickInterval;
            }

            float t = 1f - ((endTime - Time.time) / dur);
            float factor = Mathf.Lerp(bonusI, 0f, t);

            for (int i = 0; i < bulbs.Count; i++)
                bulbs[i].intensity = _originals[i].intensity + factor;

            yield return null;
        }
        RestoreOriginals();
    }

    IEnumerator FlickerRoutine(float amp, float speed, float dur)
    {
        float seed = Random.value * 100f;
        float endTime = Time.time + dur;

        while (Time.time < endTime)
        {
            float n = Mathf.PerlinNoise(seed, Time.time * speed);
            float offset = (n - 0.5f) * 2f * amp;

            for (int i = 0; i < bulbs.Count; i++)
                bulbs[i].intensity = _originals[i].intensity + offset;

            yield return null;
        }

        RestoreOriginals();
    }

    void RestoreOriginals()
    {
        for (int i = 0; i < bulbs.Count; i++)
        {
            bulbs[i].intensity = _originals[i].intensity;
            bulbs[i].pointLightOuterRadius = _originals[i].radius;
        }
    }

    void ApplyBurnTick()
    {
        if (burnDamagePerTick <= 0f) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, burnRadius, burnableMask);
        foreach (var h in hits)
        {
            var burnable = h.GetComponent<IBurnable>();
            if (burnable != null) burnable.ApplyHeat(burnDamagePerTick);
        }
    }
}