using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TailRat : RatBase
{
    [Header("Tail Attack")]
    public Transform tailPivot;

    public float tailRange = 1.5f;
    public float tailSweepAngle = 90f;
    public float tailSweepTime = 0.3f;
    public float tailCooldown = 2f;

    [Header("Damage")]
    public float damageRadius = 1.2f;

    private bool isTailAttacking = false;

    protected override void TryAttack()
    {
        if (player == null) return;
        if (isAttacking || isTailAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= tailRange)
        {
            StartCoroutine(TailAttackRoutine());
        }
    }

    IEnumerator TailAttackRoutine()
    {
        isAttacking = true;
        isTailAttacking = true;

        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.05f); //0.15f before

        float startAngle = tailSweepAngle * 0.5f;
        float endAngle = -tailSweepAngle * 0.5f;

        float timer = 0f;

        while (timer < tailSweepTime)
        {
            float t = timer / tailSweepTime;
            float angle = Mathf.Lerp(startAngle, endAngle, t);

            if (tailPivot != null)
                tailPivot.localEulerAngles = new Vector3(0, 0, angle);

            float hitDist = Vector2.Distance(player.position, transform.position);
            if (hitDist <= damageRadius)
            {
                // player.GetComponent<PlayerHealth>().TakeDamage(damage);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (tailPivot != null)
            tailPivot.localEulerAngles = Vector3.zero;

        yield return new WaitForSeconds(tailCooldown);

        isTailAttacking = false;
        isAttacking = false;
    }
}