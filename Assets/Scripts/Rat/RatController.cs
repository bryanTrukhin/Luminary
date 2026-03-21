using System.Collections;
using UnityEngine;

public class RatController : RatBase
{
    public RatStage stage;

    public enum RatStage
    {
        Sneaky,
        LongTail,
        RatKing
    }

    [Header("Stage1 Attack")]
    public float attackDistance = 1.2f;
    public float retreatSpeed = 5f;
    public float attackCooldown = 2f;

    [Header("Stage3 Rat King")]
    public float rollSpeed = 6f;
    public GameObject minionPrefab;
    public int spawnCount = 3;
    public float rollTime = 1f;

    private bool hitPlayer = false;


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

            case RatStage.RatKing:
                TryRatKingAttack();
                break;
        }
    }

    // Stage 1

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
        Vector2 attackDir = new Vector2(
            player.position.x > transform.position.x ? 1 : -1,
            0
        );

        facingDir = (int)attackDir.x;

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

    // Stage 2
    [Header("Stage2 Tail (Cone Attack)")]
    public Transform tailPivot;
    public Transform tail;
    public float tailSweepAngle = 90f;
    public float tailSweepTime = 0.3f;
    public float tailCooldown = 2f;
    public float tailRange = 1.5f;
    public LayerMask Player;
    private bool tailAttacking = false;

    bool TryTailAttack()
    {
        if (tailAttacking || player == null) return false;

        float dist = Vector2.Distance(player.position, transform.position);
        float heightDiff = Mathf.Abs(player.position.y - transform.position.y);

        if (dist <= tailRange && heightDiff <= samePlatformHeight)
        {
            isAttacking = true;
            StartCoroutine(TailAttackRoutine());
            return true;
        }

        return false;
    }
    IEnumerator TailAttackRoutine()
    {
        rb.velocity = Vector2.zero;
        tailAttacking = true;

        float startAngle = 90f;
        float endAngle = -90f;
        float timer = 0f;

        while (timer < tailSweepTime)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, timer / tailSweepTime);
            tailPivot.localEulerAngles = new Vector3(0, 0, angle);
            timer += Time.deltaTime;
            yield return null;
        }

        tailPivot.localEulerAngles = Vector3.zero;
        tailAttacking = false;
        yield return new WaitForSeconds(tailCooldown);
        isAttacking = false;
    }

        // Stage 3
        void TryRatKingAttack()
    {
        if (player == null || isAttacking) return;

        float dist = Vector2.Distance(player.position, transform.position);
        if (dist <= detectionDistance)
        {
            isAttacking = true;
            StartCoroutine(RatKingRoll());
        }
    }

    IEnumerator RatKingRoll()
    {
        hitPlayer = false;
        Vector2 dir = player.position.x > transform.position.x ? Vector2.right : Vector2.left;
        facingDir = (int)dir.x;

        float t = 0f;
        while (t < rollTime)
        {
            rb.velocity = dir * rollSpeed;
            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;

        SpawnMinions();

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    void SpawnMinions()
    {
        if (minionPrefab == null) return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 1.0f;
            Vector2 spawnPos = (Vector2)transform.position + offset;

            RaycastHit2D hit = Physics2D.Raycast(spawnPos, Vector2.down, 2f, LayerMask.GetMask("Platfrom"));
            if (hit)
                spawnPos.y = hit.point.y + 0.1f; 

            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);

            MinionRat mr = minion.GetComponent<MinionRat>();
            if (mr != null && player != null)
                mr.AssignPlayer(player);
        }
    }
}
