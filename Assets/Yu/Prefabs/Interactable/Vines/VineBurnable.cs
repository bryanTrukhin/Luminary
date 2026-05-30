using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class VineBurnable : MonoBehaviour, IBurnable
{
    public enum Orientation{
        Left,
        Right
    }

    public Orientation burnableDirection = Orientation.Left;
    [Header("Health")]
    [SerializeField] private float maxHealth = 3f;
    private float _health;

    [Header("FX")]
    [SerializeField] private GameObject burnVfxPrefab;   // optional
    [SerializeField] private AudioClip burnSfx;          // optional
    [SerializeField] private float burnSfxVolume = 0.7f;

    private bool _dead;

    void Awake()
    {
        _health = maxHealth;

        // Make sure collider is trigger so it doesn't block the player (optional)
        var col = GetComponent<Collider2D>();
        col.isTrigger = false;
    }

    public void ApplyHeat(float amount)
    {
        float diff = transform.position.x - GameObject.FindWithTag("Player").transform.position.x;
        if (burnableDirection == Orientation.Left  && diff < 0) return;
        if (burnableDirection == Orientation.Right && diff > 0) return;
        if (_dead) return;

        _health -= amount;

        if (_health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        _dead = true;
        
        if (burnVfxPrefab != null && PoolManager.instance != null)
        {
            PoolManager.instance.ReuseObject(burnVfxPrefab, transform.position, Quaternion.identity);
        }
        
        if (burnSfx != null)
        {
            AudioSource.PlayClipAtPoint(burnSfx, transform.position, burnSfxVolume);
        }
        gameObject.SetActive(false);
    }
}