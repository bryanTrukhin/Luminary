using System.Collections;
using UnityEngine;

namespace SupanthaPaul
{
    [RequireComponent(typeof(Collider2D))]
    public class HideMonster : MonoBehaviour
    {
        [Header("Path")]
        [SerializeField] private Transform spawnPoint;   // far right
        [SerializeField] private Transform stopPoint;    // where it stops / despawns
        [SerializeField] private float moveSpeed = 20f;  // units/sec, tweak for “very fast”

        [Header("FX")]
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] private AudioSource sfx;        // optional, for screech / roar

        private bool _active;
        private PlayerController _targetPlayer;

        void Awake()
        {
            // ensure collider is trigger
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            // Start inactive in scene if you want
            gameObject.SetActive(false);
        }

        public void Trigger(PlayerController player)
        {
            if (_active) return;

            _active = true;
            _targetPlayer = player;

            if (spawnPoint != null)
                transform.position = spawnPoint.position;

            gameObject.SetActive(true);

            if (smoke != null) smoke.Play();
            if (sfx != null)   sfx.Play();
        }

        void Update()
        {
            if (!_active) return;
            if (stopPoint == null)
            {
                Debug.LogWarning("HideMonster: stopPoint not assigned.", this);
                return;
            }

            // Move toward stop point
            transform.position = Vector3.MoveTowards(
                transform.position,
                stopPoint.position,
                moveSpeed * Time.deltaTime
            );

            // Reached end → despawn
            if (Vector3.Distance(transform.position, stopPoint.position) < 0.05f)
            {
                Deactivate();
            }
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (!_active) return;
            if (!col.CompareTag("Player")) return;

            var p = col.GetComponentInParent<PlayerController>();
            if (p == null) return;

            // If controller exists and says player is hiding, do nothing
            if (GameController.I != null && GameController.I.PlayerSafeFromMonster())
            {
                Debug.Log("HideMonster: player was hidden, no kill.");
                return;
            }

            // Otherwise: kill
            Debug.Log("HideMonster: player hit, attempt kill.");
            if (GameController.I != null)
            {
                GameController.I.KillPlayer(p);
            }
            else
            {
                // fallback – just freeze player
                p.SetMoveable(false);
            }

            // After killing, the monster can despawn or keep going, your choice:
            Deactivate();
        }

        void Deactivate()
        {
            _active = false;
            if (smoke != null) smoke.Stop();
            gameObject.SetActive(false);
        }
    }
}
