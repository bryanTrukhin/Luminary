using UnityEngine;

[DisallowMultipleComponent]
public class BrokenLanternController : MonoBehaviour, ILantern
{
    [Header("Decay Settings")]
    [SerializeField] private float fireflyCapacity = 100f;
    [SerializeField] private float decayRate = 2f;
    private float _currentFireflies;

    [Header("Movement Costs")]
    [SerializeField] private float dashCost;
    [SerializeField] private float doubleJumpCost;

    [Header("Visuals")]
    [SerializeField] private HingeJoint2D topChainJoint;
    private float _baseAnchorX;

    private void Start(){
        _baseAnchorX = Mathf.Abs(topChainJoint.connectedAnchor.x);
        ResetHealth();
    }

    private void Update()
    {
        // Perpetually decay
        if (_currentFireflies > 0)
        {
            _currentFireflies -= decayRate * Time.deltaTime;
            
            if (_currentFireflies <= 0)
            {
                _currentFireflies = 0;
                GameController.I.KillPlayer(GetComponentInParent<PlayerController>());
            }
        }
    }

    public bool TryConsumeEnergy(float amount)
    {
        if (_currentFireflies >= amount)
        {
            _currentFireflies -= amount;
            return true;
        }
        return false;
    }

    public bool TryUseDash() => TryConsumeEnergy(dashCost);
    
    public bool TryUseDoubleJump() => TryConsumeEnergy(doubleJumpCost);

    // ================= Disabled Non-Movement Abilities ================= //

    public void TriggerFlash() 
    { 
        // Intentionally blank. Lantern is broken.
    }
    public void TriggerFireflyBomb() 
    { 
        // Intentionally blank. Lantern is broken.
    }
    public void TakeDamage(float amount, int health_amount = 1)
    {
        if (GameController.I.State == PlayState.Dead) return;

        _currentFireflies -= amount;
        Flicker(0.5f, 20f, 0.5f); 

        if (_currentFireflies <= 0)
        {
            _currentFireflies = 0;
            GameController.I.KillPlayer(GetComponentInParent<PlayerController>());
        }
    }

    public void ResetHealth() => _currentFireflies = fireflyCapacity;

    public void SetVisible(bool state)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = state;
    }

    public void Flicker(float amplitude, float speed, float duration)
    {
        // basic sprite flicker logic
        // stripped of complex Light2D bulb loops since it is broken.
    }

        public void UpdateFacingDirection(bool facingLeft)
    {
        Vector2 currentAnchor = topChainJoint.connectedAnchor;
        currentAnchor.x = facingLeft ? -_baseAnchorX : _baseAnchorX;
        topChainJoint.connectedAnchor = currentAnchor;
    }

    public void RegenFireflies(float rate)
    {
        _currentFireflies += rate * Time.deltaTime;
        _currentFireflies = Mathf.Min(_currentFireflies, fireflyCapacity);
    }

}