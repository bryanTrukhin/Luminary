using System.Collections;
using UnityEngine;

namespace SupanthaPaul
{
    public class HideMonsterTrigger : MonoBehaviour
    {
        [SerializeField] private HideMonster monster;
        [SerializeField] private float preDelay = 0.75f; // time between flicker and monster spawning
        [Header("Lantern Flicker")]
        [SerializeField] private float flickerAmplitude = 0.9f;
        [SerializeField] private float flickerSpeed     = 10f;
        [SerializeField] private float flickerDuration  = 0.8f;

        private bool _used;

        void OnTriggerEnter2D(Collider2D col)
        {
            if (_used) return;
            if (!col.CompareTag("Player")) return;

            var player = col.GetComponentInParent<PlayerController>();
            if (player == null) return;
            if (monster == null)
            {
                Debug.LogError("HideMonsterTrigger: monster reference not set.", this);
                return;
            }

            _used = true;
            Debug.Log("HideMonsterTrigger: triggered by player.");

            // 1) Lantern flicker warning
            if (player.Lantern != null)
            {
                player.Lantern.Flicker(flickerAmplitude, flickerSpeed, flickerDuration);
            }

            // 2) After short delay, spawn monster
            StartCoroutine(SpawnAfterDelay(player));
        }

        IEnumerator SpawnAfterDelay(PlayerController player)
        {
            yield return new WaitForSeconds(preDelay);
            monster.Trigger(player);
        }
    }
}
