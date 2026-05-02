using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SneakyRat : RatBase
{
    public float attackDistance = 1.2f;
    public float retreatSpeed = 5f;
    public float attackCooldown = 2f;

    protected override void TryAttack()
    {
        if (player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackDistance)
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        isAttacking = true;

        Vector2 dir = player.position.x > transform.position.x ? Vector2.right : Vector2.left;

        rb.velocity = dir * chaseSpeed;
        yield return new WaitForSeconds(0.15f);

        rb.velocity = -dir * retreatSpeed;
        yield return new WaitForSeconds(0.5f);

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }
}
