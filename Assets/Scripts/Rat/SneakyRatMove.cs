using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SneakyRatMove : RatBase
{
    [Header("Attack")]
    public float attackDistance = 1.2f;
    public float retreatDistance = 3f;
    public float attackCooldown = 2f;
    public float retreatSpeed = 5f; 

    protected override void TryAttack()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        float heightDiff = Mathf.Abs(player.position.y - transform.position.y);

        if (dist <= attackDistance && heightDiff < samePlatformHeight)
            StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        Vector2 attackDir = (player.position - transform.position).normalized;
        rb.velocity = attackDir * chaseSpeed;

        // 冲刺短时间
        float timer = 0f;
        while (Vector2.Distance(transform.position, player.position) > 0.5f && timer < 0.2f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 攻击触发（可加动画或伤害逻辑）
        Debug.Log("Stage1 SneakyRat ATTACK!");

        // 快速后退
        Vector2 retreatDir = -attackDir;
        while (Vector2.Distance(transform.position, player.position) < retreatDistance)
        {
            rb.velocity = retreatDir * retreatSpeed;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }
}