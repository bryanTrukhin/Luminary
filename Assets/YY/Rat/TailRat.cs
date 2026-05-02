using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TailRat : RatBase
{
    public float tailRange = 1.5f;
    public float cooldown = 2f;

    protected override void TryAttack()
    {
        if (player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < tailRange)
        {
            StartCoroutine(TailAttack());
        }
    }

    IEnumerator TailAttack()
    {
        isAttacking = true;

        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.2f);

        Debug.Log("TAIL ATTACK");

        yield return new WaitForSeconds(cooldown);

        isAttacking = false;
    }
}
