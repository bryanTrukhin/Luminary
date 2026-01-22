using UnityEngine;

namespace SupanthaPaul
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class DarknessWall : MonoBehaviour
    {
        [Header("Path")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        [Header("Movement")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private bool autoStart = true;

        private bool _active;
        private float _t;   // 0 → 1 along the path

        void Awake()
        {
            // ensure trigger / kinematic
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        void OnEnable()
        {
            if (autoStart)
                Begin();
        }

        /// <summary>
        /// Reset to startPoint and begin moving toward endPoint.
        /// </summary>
        public void Begin()
        {
            if (startPoint == null || endPoint == null)
            {
                Debug.LogError("DarknessWall: startPoint or endPoint is not assigned!", this);
                _active = false;
                return;
            }

            transform.position = startPoint.position;
            _t = 0f;
            _active = true;
        }

        void Update()
        {
            if (!_active) return;
            if (startPoint == null || endPoint == null) return;

            // distance along path
            float dist = Vector2.Distance(startPoint.position, endPoint.position);
            if (dist <= 0.001f)
            {
                _active = false;
                return;
            }

            // move at 'speed' units per second along the line
            float delta = speed * Time.deltaTime / dist;
            _t += delta;
            _t = Mathf.Clamp01(_t);

            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, _t);

            if (_t >= 1f)
            {
                // reached end; stop here
                _active = false;
            }
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player")) return;

            var player = col.GetComponentInParent<PlayerController>();
            if (player == null) return;

            // If player is hiding in a locker, ignore
            if (GameController.I != null && GameController.I.PlayerSafeFromMonster())
                return;

            // Kill the player (jumpscare, etc.)
            if (GameController.I != null)
            {
                GameController.I.KillPlayer(player);
            }
            else
            {
                player.SetPlayable(false);
            }
        }
    }
}
