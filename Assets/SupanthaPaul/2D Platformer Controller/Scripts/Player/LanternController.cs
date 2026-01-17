using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SupanthaPaul
{
    [DisallowMultipleComponent]
    public class LanternController : MonoBehaviour
    {

        [Header("Burning")]
        [SerializeField] private LayerMask burnableMask;
        [SerializeField] private float burnRadius         = 1.5f;
        [SerializeField] private float burnDamagePerTick  = 1f;
        [SerializeField] private float burnTickInterval   = 0.1f;

        /* ------------------------------------------------- CONFIG --------- */
        [Tooltip("Leave empty → auto-collect every Light2D under this object")]
        [SerializeField] private List<Light2D> bulbs = new();

        [Tooltip("Transform whose localScale.x tells us facing direction.")]
        [SerializeField] private Transform playerRoot;   // auto-filled to parent

        [Header("Resting Offsets (local space)")]
        [SerializeField] private Vector3 rightRest = new Vector3( 0.55f, -0.20f, 0);
        [SerializeField] private Vector3 leftRest  = new Vector3(-0.55f, -0.20f, 0);

        [Header("Swing settings")]
        [Tooltip("Time (seconds) it takes to settle ~63 % towards the target offset")]
        [SerializeField] private float smoothTime = 0.08f;   // lower = snappier

        /* ------------------------------------------------- STATE ---------- */
        // cached originals so we never damage your tuning
        struct BulbState { public float intensity; public float radius; }
        private readonly List<BulbState> _originals = new();

        private Vector3 _swingVelocity;       // for SmoothDamp
        private Coroutine _flickerCo;         // keep one flicker at a time

        /* ------------------------------------------------- LIFECYCLE ------ */
        void Awake()
        {
            if (bulbs.Count == 0)
                bulbs.AddRange(GetComponentsInChildren<Light2D>(includeInactive: true));

            if (playerRoot == null)
                playerRoot = transform.parent;

            foreach (var b in bulbs)
                _originals.Add(new BulbState
                {
                    intensity = b.intensity,
                    radius    = b.pointLightOuterRadius
                });

            // place correctly at start
            bool facingRight = playerRoot.localScale.x >= 0f;
            transform.localPosition = facingRight ? rightRest : leftRest;
        }

        void LateUpdate()   // run after PlayerController.Flip()
        {
            bool facingRight     = playerRoot.localScale.x >= 0f;
            Vector3 targetOffset = facingRight ? rightRest : leftRest;

            transform.localPosition =
                Vector3.SmoothDamp(transform.localPosition,
                                   targetOffset,
                                   ref _swingVelocity,
                                   smoothTime);
        }

        /* ------------------------------------------------- API ------------ */

        /// <summary>Enable/disable all lantern bulbs.</summary>
        public void SetOn(bool on)
        {
            foreach (var b in bulbs) b.enabled = on;
        }

        /// <summary>
        /// Flash: quick, bright burst that decays over <paramref name="duration"/>.
        /// Useful for the "scare away stalker" mechanic.
        /// </summary>
        public void Flash(float bonusIntensity, float duration)
        {
            if (_flickerCo != null) StopCoroutine(_flickerCo);
            _flickerCo = StartCoroutine(FlashRoutine(bonusIntensity, duration));
        }

        /// <summary>
        /// Flicker your tuned lights without altering the mean intensity.
        /// amplitude = peak deviation **above & below** baseline.
        /// </summary>
        public void Flicker(float amplitude, float speed, float duration)
        {
            if (_flickerCo != null) StopCoroutine(_flickerCo);
            _flickerCo = StartCoroutine(FlickerRoutine(amplitude, speed, duration));
        }

        /* ------------------------------------------------- COROUTINES ----- */
        IEnumerator FlashRoutine(float bonusI, float dur)
        {
            float endTime = Time.time + dur;
            float nextTickTime = Time.time;   // first tick immediately

            while (Time.time < endTime)
            {
                if (Time.time >= nextTickTime)
                {
                    ApplyBurnTick();
                    nextTickTime += burnTickInterval;
                }
                float t = 1f - ((endTime - Time.time) / dur); // 0→1
                float factor = Mathf.Lerp(bonusI, 0f, t);      // decay to 0

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
                float n = Mathf.PerlinNoise(seed, Time.time * speed);   // 0‒1
                float offset = (n - 0.5f) * 2f * amp;                   // -amp‒amp

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
                bulbs[i].intensity             = _originals[i].intensity;
                bulbs[i].pointLightOuterRadius = _originals[i].radius;
            }
        }

        void ApplyBurnTick()
        {
            if (burnDamagePerTick <= 0f) return;

            // Circle around the lantern's position
            var hits = Physics2D.OverlapCircleAll(transform.position, burnRadius, burnableMask);
            foreach (var h in hits)
            {
                // look for anything that can be burned
                var burnable = h.GetComponent<IBurnable>();
                if (burnable != null)
                {
                    burnable.ApplyHeat(burnDamagePerTick);
                }
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // visualize burn radius in editor
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, burnRadius);
        }
        #endif
    }
}
