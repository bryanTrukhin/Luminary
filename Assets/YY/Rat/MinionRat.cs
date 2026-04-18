using System.Collections;
using UnityEngine;

public class MinionRat : RatBase
{
    public RatStage stage;

    public enum RatStage
    {
        Sneaky,
        LongTail
    }

    [Header("Stage1 Attack")]
    public float attackDistance = 1.2f;
    public float retreatSpeed = 5f;
    public float attackCooldown = 2f;

    [Header("Stage2 Tail")]
    public Collider2D tailCollider;
    public float tailAttackRange = 1.5f;

    [Header("Tail Cone Attack")]
    public float tailRange = 2f;
    public float tailAngle = 90f;
    public float tailCooldown = 2f;
    public float tailWindup = 0.15f;
    public LayerMask playerLayer;

    private bool hitPlayer = false;

    protected override void Start()
    {
        base.Start();

        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }
        else
        {
            Debug.LogWarning("MinionRat: No GameObject with tag 'Player' found!");
        }
    }
    public void AssignPlayer(Transform p)
    {
        player = p;
    }

    protected override void TryAttack()
    {
        if (player == null || isAttacking) return;

        switch (stage)
        {
            case RatStage.Sneaky:
                TrySneakyAttack();
                break;

            case RatStage.LongTail:
                if (!TryTailAttack())
                    TrySneakyAttack();
                break;
        }
    }

    // Stage1
    void TrySneakyAttack()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        float heightDiff = Mathf.Abs(player.position.y - transform.position.y);

        if (dist <= attackDistance && heightDiff < samePlatformHeight)
        {
            isAttacking = true;
            StartCoroutine(SneakyAttackRoutine());
        }
    }

    IEnumerator SneakyAttackRoutine()
    {
        Vector2 attackDir = (player.position.x > transform.position.x ? Vector2.right : Vector2.left);
        facingDir = (int)attackDir.x;

        Debug.Log("Stage1 DASH!");
        rb.velocity = attackDir * chaseSpeed;
        yield return new WaitForSeconds(0.15f);

        Vector2 retreatDir = -attackDir;
        facingDir = (int)retreatDir.x;

        float t = 0f;
        float retreatTime = 0.5f;

        while (t < retreatTime)
        {
            rb.velocity = new Vector2(retreatDir.x * retreatSpeed, rb.velocity.y);
            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    // Stage2
    bool TryTailAttack()
    {
        float dist = Vector2.Distance(player.position, transform.position);
        float heightDiff = Mathf.Abs(player.position.y - transform.position.y);

        if (dist < tailRange && heightDiff < samePlatformHeight)
        {
            isAttacking = true;
            StartCoroutine(TailAttackRoutine());
            return true;
        }

        return false;
    }

    void TailConeAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, tailRange, playerLayer);

        foreach (Collider2D hit in hits)
        {
            Vector2 dirToPlayer = (hit.transform.position - transform.position).normalized;
            Vector2 forward = new Vector2(facingDir, 0);
            float angle = Vector2.Angle(forward, dirToPlayer);

            if (angle <= tailAngle * 0.5f)
            {
                //Debug.Log($"TAIL HIT {hit.name} | angle={angle}");
                // hit.GetComponent<Player>().TakeDamage(1);
            }
        }
    }

    IEnumerator TailAttackRoutine()
    {
        rb.velocity = Vector2.zero;

        facingDir = (player.position.x > transform.position.x ? 1 : -1);

        Debug.Log("TAIL ATTACK WINDUP");
        yield return new WaitForSeconds(tailWindup);

        TailConeAttack();

        yield return new WaitForSeconds(tailCooldown);

        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 forward = new Vector3(facingDir, 0, 0);
        Gizmos.DrawWireSphere(transform.position, tailRange);

        Vector3 left = Quaternion.Euler(0, 0, tailAngle / 2) * forward;
        Vector3 right = Quaternion.Euler(0, 0, -tailAngle / 2) * forward;

        Gizmos.DrawLine(transform.position, transform.position + left * tailRange);
        Gizmos.DrawLine(transform.position, transform.position + right * tailRange);
    }
}
