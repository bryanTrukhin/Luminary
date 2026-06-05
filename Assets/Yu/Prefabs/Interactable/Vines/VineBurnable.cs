using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VineBurnable : MonoBehaviour, IBurnable
{
    public enum Orientation
    {
        Left,
        Right
    }

    [Header("Persistence")]
    [Tooltip("Must be unique for each vine in the scene.")]
    [SerializeField] private string vineID;

    public Orientation burnableDirection = Orientation.Left;
    [Header("Health")]
    [SerializeField] private float maxHealth = 3f;
    private float _health;

    [Header("FX")]
    [SerializeField] private GameObject burnVfxPrefab;
    [SerializeField] private AudioClip burnSfx;
    [SerializeField] private float burnSfxVolume = 0.7f;

    private bool _dead;

    void Awake()
    {
        if (PlayerPrefs.GetInt(vineID + "_destroyed", 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        _health = maxHealth;
        GetComponent<Collider2D>().isTrigger = false;
    }

    public void ApplyHeat(float amount)
    {
        float diff = transform.position.x - GameObject.FindWithTag("Player").transform.position.x;
        if (burnableDirection == Orientation.Left && diff < 0) return;
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
        PlayerPrefs.SetInt(vineID + "_destroyed", 1);
        PlayerPrefs.Save();

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