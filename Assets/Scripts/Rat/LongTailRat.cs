using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongTailRat :SneakyRatMove
{
    [Header("Tail Attack")]
    public Collider2D tailCollider;
    public float tailDamage = 2f;

    protected override void TryAttack()
    {
        base.TryAttack();

        if (player == null) return;

        float dist = Vector2.Distance(player.position, transform.position);
        float heightDiff = Mathf.Abs(player.position.y - transform.position.y);

        if (dist < 1.5f && heightDiff < samePlatformHeight)
        {
            Debug.Log("Stage2 LongTailRat TAIL ATTACK!");
            if (tailCollider != null) tailCollider.enabled = true;
            StartCoroutine(DisableTailCollider(0.3f));
        }
    }

    IEnumerator DisableTailCollider(float time)
    {
        yield return new WaitForSeconds(time);
        if (tailCollider != null) tailCollider.enabled = false;
    }
}

