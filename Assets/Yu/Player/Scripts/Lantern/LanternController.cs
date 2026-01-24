using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

[DisallowMultipleComponent]
public class LanternController : MonoBehaviour, ILantern
{
    [Header("Firefly Currency")]
    [SerializeField] private float fireflyCapacity = 100f;
    [SerializeField] private float fireflyRegenRate = 5f;
    private float _currentFireflies;
    public TMP_Text currentFireFlyCountUI;

    [Header("Lantern Flash Ability")]
    [SerializeField] private float flashFireflyCost = 20f;
    [SerializeField] private float flashBonusIntensity = 2.5f;
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private float flashCooldown = 0.5f;
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

    void Awake()
    {
        _currentFireflies = fireflyCapacity;

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
        currentFireFlyCountUI.text = "Fireflies: " + _currentFireflies.ToString("0.0");
        
        // 1. Handle Cooldowns internally
        if (_flashCooldownTimer > 0) _flashCooldownTimer -= Time.deltaTime;

        // 2. Regenerate Fireflies
        if (_currentFireflies < fireflyCapacity && fireflyRegenRate > 0f)
        {
            _currentFireflies += fireflyRegenRate * Time.deltaTime;
            _currentFireflies = Mathf.Min(_currentFireflies, fireflyCapacity);
        }
    }

    void LateUpdate()
    {
        // Handle Swing Logic
        bool facingRight = playerRoot.localScale.x >= 0f;
        Vector3 targetOffset = facingRight ? rightRest : leftRest;
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetOffset, ref _swingVelocity, smoothTime);
    }

    // ================= INTERFACE IMPLEMENTATION ================= //

    /// <summary>
    /// Attempts to consume fireflies for an action.
    /// </summary>
    public bool TryConsumeEnergy(float amount) // Kept method name from Interface for compatibility
    {
        if (_currentFireflies >= amount)
        {
            _currentFireflies -= amount;
            return true;
        }
        
        Debug.Log("Not enough Fireflies!"); 
        return false;
    }

    public void TriggerFlash()
    {
        // Check Cooldown AND Firefly count
        if (_flashCooldownTimer <= 0f && TryConsumeEnergy(flashFireflyCost))
        {
            Flash(flashBonusIntensity, flashDuration);
            _flashCooldownTimer = flashCooldown;
        }
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

    // ================= INTERNAL LOGIC & COROUTINES ================= //

    private void Flash(float bonusIntensity, float duration)
    {
        if (_flickerCo != null) StopCoroutine(_flickerCo);
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

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, burnRadius);
    }
    #endif
}