using System.Collections;
using UnityEngine;

public class FireflyPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float restoreAmount = 25f; // Amount of fireflies to regen
    
    [Header("Visual Effects")]
    [SerializeField] private float effectDuration = 0.4f; // How long the fade/scale takes
    [SerializeField] private float targetScaleX = 2.5f;   // How wide it stretches

    private SpriteRenderer _sr;
    private Collider2D _col;
    private bool _collected = false;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent double pickups
        if (_collected) return;

        // Check if the object colliding is the player
        // We use the PlayerController you provided to find the link
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            // The PlayerController has an ILantern interface. 
            // Since ILantern doesn't have a 'Refill' method in your interface yet,
            // we cast it to the concrete LanternController to access the variable directly.
            if (player.Lantern is LanternController lanternScript)
            {
                Collect(lanternScript);
            }
        }
    }

    private void Collect(LanternController lantern)
    {
        _collected = true;
        _col.enabled = false; // Disable collision immediately so we don't trigger it again

        // 1. Regen the fireflies
        // Note: We access the public variable directly as this is a temp script.
        // Ideally, you'd add a public void Restore(float amount) to LanternController later.
        lantern._currentFireflies += restoreAmount;

        // 2. Start the visual effect
        StartCoroutine(PickupRoutine());
    }

    private IEnumerator PickupRoutine()
    {
        float timer = 0f;
        
        Vector3 initialScale = transform.localScale;
        Color initialColor = _sr.color;

        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float t = timer / effectDuration; // Normalized time (0 to 1)

            // Lerp Scale: Stretch X, keep Y and Z the same
            float newX = Mathf.Lerp(initialScale.x, targetScaleX, t);
            transform.localScale = new Vector3(newX, initialScale.y, initialScale.z);

            // Lerp Alpha: Fade to 0
            float newAlpha = Mathf.Lerp(initialColor.a, 0f, t);
            _sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, newAlpha);

            yield return null;
        }

        // Ensure it's fully invisible/done before destroying
        Destroy(gameObject);
    }
}