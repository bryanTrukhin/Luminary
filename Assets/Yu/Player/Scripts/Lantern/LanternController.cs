using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

[DisallowMultipleComponent]
public class LanternController : MonoBehaviour, ILantern
{
    
    [Header("Firefly")]
    [SerializeField] private float fireflyCapacity;
    [SerializeField] private float fireflyStarting;
    [SerializeField] private float fireflyRegenRate;
    [SerializeField] private Transform parent;
    public float _currentFireflies;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 1.0f; // I-Frames
    [SerializeField] private bool freezeFrame;
    private float _damageTimer;

    [Header("Movement Abilities")]
    [SerializeField] private float dashCost;
    [SerializeField] private float doubleJumpCost;

    [Header("Firefly Flash")]
    [SerializeField] private float flashLanternCost;
    [SerializeField] private float flashBonusIntensity;
    [SerializeField] private float flashDuration;
    [SerializeField] public float flashCooldown;
    
    [Header("Firefly Bomb")]
    [SerializeField] private GameObject fireflyBomb;
    [SerializeField] private float projectileCost = 10f;
    [SerializeField] private float projectileCooldown = 3f;
    public float bombCdTimer;

    
    public float _flashCooldownTimer;
    
    [Header("Vine Burning Mechanics")]
    [SerializeField] private LayerMask burnableMask;
    [SerializeField] private float burnRadius = 1.5f;
    [SerializeField] private float burnDamagePerTick = 1f;
    [SerializeField] private float burnTickInterval = 0.1f;

    [Header("Visuals")]
    public TMP_Text currentFireFlyCountUI;
    public CooldownUI flashUI;
    public CooldownUI dashUI;
    public CooldownUI flashAreaUI;
    [SerializeField] private List<Light2D> bulbs = new();
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Vector3 frontOffset = new Vector3(0.55f, -0.20f, 0);
    [SerializeField] private float smoothTime = 0.08f;
    [SerializeField] private HingeJoint2D topChainJoint;
    private float _baseAnchorX;
    private Rigidbody2D _rb;
    private PlayerController _pc;
    [SerializeField] private Rigidbody2D handPosRB;

    [SerializeField] private Animator _animator;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flashSound;
    [SerializeField] private AudioClip dashSound;


    // Internal State
    struct BulbState { public float intensity; public float radius; }
    private readonly List<BulbState> _originals = new();
    private Vector3 _swingVelocity;
    private Coroutine _flickerCo;
    private bool _wasFacingRight = true;
    
    // make ability to spend some fireflies to enable regeneration
    // Likely need to make this inumerator
    
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
        
        // Initial position (parent's scale flip mirrors children automatically)
        transform.localPosition = frontOffset;
        _rb = playerRoot.GetComponent<Rigidbody2D>();
        _pc = playerRoot.GetComponent<PlayerController>();
        _baseAnchorX = Mathf.Abs(topChainJoint.connectedAnchor.x);
        _wasFacingRight = playerRoot.localScale.x >= 0f;
        freezeFrame = false;
    }
    void FixedUpdate()
    {
        if (playerRoot == null || handPosRB == null) return;

        bool isFacingRight = playerRoot.localScale.x > 0 && playerRoot.right.x > 0;
        float side = isFacingRight ? 1f : -1f;

        Vector3 targetOffset = new Vector3(frontOffset.x * side, frontOffset.y, frontOffset.z);
        Vector3 targetWorldPos = playerRoot.position + targetOffset;
        if ((side > 0) != _wasFacingRight)
        {
            _swingVelocity = Vector3.zero;
            _wasFacingRight = (side > 0);
        }

        Vector3 newPos = Vector3.SmoothDamp(handPosRB.position, targetWorldPos, ref _swingVelocity, smoothTime);
        handPosRB.MovePosition(newPos);
    }
    void Update()
    {
        currentFireFlyCountUI.text = _currentFireflies.ToString("0.0") + "/" + fireflyCapacity;

        if (_flashCooldownTimer > 0) _flashCooldownTimer -= Time.deltaTime;
        if (bombCdTimer > 0) bombCdTimer -= Time.deltaTime;
        if (_damageTimer > 0f) _damageTimer -= Time.deltaTime;

        if (_currentFireflies < fireflyCapacity && fireflyRegenRate > 0f)
        {
            _currentFireflies += fireflyRegenRate * Time.deltaTime;
            _currentFireflies = Mathf.Min(_currentFireflies, fireflyCapacity);
        }
        
        if (InputSystem.FlashLantern())
        {
            TriggerFlash();
        }
        if (InputSystem.FlashArea())
        {
            TriggerFireflyBomb();
        }
    }
    public bool TryConsumeEnergy(float amount)
    {
        if (_currentFireflies >= amount)
        {
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
        if(!freezeFrame){
            freezeFrame = true;
            StartCoroutine(FrameFreeze(.25f));
        }
        if (playerSprite != null) StartCoroutine(FrameFreezeSprite(damageCooldown));

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

    public IEnumerator FrameFreeze(float duration)
    {
        Debug.Log("Frame Freeze Triggered!");
        if (!freezeFrame) yield break;
        float timeScale = Time.timeScale;

        Time.timeScale = 0.01f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = timeScale;
        freezeFrame = false;
    }

    private IEnumerator FrameFreezeSprite(float duration)
    {
        float endTime = Time.time + duration;
        Color damageColor = Color.red;
        Color normalColor = Color.white;

        while (Time.time < endTime)
        {
            playerSprite.color = damageColor;
            yield return new WaitForSecondsRealtime(0.1f);
            playerSprite.color = normalColor;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        playerSprite.color = normalColor; // Reset
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
        if (_flashCooldownTimer <= 0f && result)
        {
             _currentFireflies -= flashLanternCost;
            Flash(flashBonusIntensity, flashDuration);
            if (flashSound)
            {
                audioSource.clip = flashSound;
                audioSource.Play();
            }
            _flashCooldownTimer = flashCooldown;
        }
    }

    public bool TryUseDash()
    {
        Debug.Log("Playing Dash Sound");
        bool result = TryConsumeEnergy(dashCost);
        if (result)
        {
            _currentFireflies -= dashCost;
            if (dashSound)
            {
                audioSource.clip = dashSound;
                audioSource.Play();
            }
            
            if (dashUI != null) 
            {
                dashUI.TriggerDashCooldown();
            }
        }
        return result;
    }



    public bool TryUseDoubleJump()
    {
        bool result = TryConsumeEnergy(doubleJumpCost);
        return result;
    }

    // ================= Lantern Visuals and COROUTINES ================= //
    // public void UpdateFacingDirection(bool facingLeft)
    // {
    //     Vector2 currentAnchor = topChainJoint.connectedAnchor;
    //     currentAnchor.x = facingLeft ? -_baseAnchorX : _baseAnchorX;
    //     topChainJoint.connectedAnchor = currentAnchor;
    // }

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
        if(_animator) _animator.SetTrigger("isFlashing");
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
        if(_animator) _animator.SetTrigger("isFlashing");
        RestoreOriginals();
    }

    // IEnumerator AreaFlashRoutine(float bonusI, float dur)
    // {
    //     float endTime = Time.time + dur;
    //
    //     if(_animator) _animator.SetTrigger("isFlashing");
    //     while (Time.time < endTime)
    //     {
    //         float t = 1f - ((endTime - Time.time) / dur);
    //         float factor = Mathf.Lerp(bonusI, 0f, t);
    //
    //         for (int i = 0; i < bulbs.Count; i++)
    //             bulbs[i].intensity = _originals[i].intensity + factor;
    //
    //         yield return null;
    //     }
    //     if(_animator) _animator.SetTrigger("isFlashing");
    //     RestoreOriginals();
    // }

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

        // Use the physical lantern position instead of the script's transform
        Vector2 checkPos = handPosRB != null ? handPosRB.position : (Vector2)transform.position;

        var hits = Physics2D.OverlapCircleAll(checkPos, burnRadius, burnableMask);

        foreach (var h in hits)
        {
            var burnable = h.GetComponent<IBurnable>();
            if (burnable != null) burnable.ApplyHeat(burnDamagePerTick);
        }
    }
    public void TriggerFireflyBomb()
    {
        if (bombCdTimer > 0f || !TryConsumeEnergy(projectileCost)) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        GameObject proj = Instantiate(fireflyBomb, handPosRB.position, Quaternion.identity);
        proj.GetComponent<ProjectileScript>().Init(mouseWorld);

        _currentFireflies -= projectileCost;
        bombCdTimer = projectileCooldown;
    }
}