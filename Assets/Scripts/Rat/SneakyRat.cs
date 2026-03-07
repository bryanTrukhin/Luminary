using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SneakyRat : EnemyBase
{
    [Header("Movement")]
    public float sneakSpeed = 1.5f;
    public float chaseSpeed = 3f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float edgeCheckOffset = 0.5f; 
    public float wallCheckDistance = 0.2f;

    [Header("FOV")]
    public float viewDistance = 5f;
    public LayerMask obstacleMask;

    private Rigidbody2D rb;
    private float moveDir = 1f;
    private float lastFlipTime;
    public float flipCooldown = 0.25f;

    enum RatState { Sneak, Chase }
    RatState state = RatState.Sneak;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        moveDir = Random.value > 0.5f ? 1 : -1;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        bool canSee = CanSeePlayer();
        state = canSee ? RatState.Chase : RatState.Sneak;

        if (state == RatState.Chase)
        {
            float dirToPlayer = (player.position.x > transform.position.x) ? 1 : -1;
            if (dirToPlayer != moveDir && GroundAhead(dirToPlayer))
            {
                moveDir = dirToPlayer;
            }
        }

        Move();
    }

    void Move()
    {
        float speed = (state == RatState.Sneak) ? sneakSpeed : chaseSpeed;

        if ((!GroundAhead(moveDir) || WallAhead()) && Time.time > lastFlipTime + flipCooldown)
        {
            Flip();
        }

        rb.velocity = new Vector2(moveDir * speed, rb.velocity.y);
        UpdateFacing();
    }

    bool GroundAhead(float direction)
    {
        Vector2 checkPos = (Vector2)transform.position + new Vector2(direction * edgeCheckOffset, -0.6f);

        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.15f, groundLayer);

        Debug.DrawLine((Vector2)transform.position, checkPos, hit ? Color.green : Color.red);
        return hit != null;
    }

    bool WallAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0, 0.2f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * moveDir, wallCheckDistance, groundLayer);

        return hit.collider != null;
    }

    void Flip()
    {
        moveDir *= -1;
        lastFlipTime = Time.time;
    }

    void UpdateFacing()
    {
        if (moveDir != 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * moveDir, transform.localScale.y, transform.localScale.z);
        }
    }

    bool CanSeePlayer()
    {
        Vector2 dir = player.position - transform.position;

        if (dir.magnitude > viewDistance) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, viewDistance, obstacleMask);

        if (hit.collider != null && hit.collider.transform != player)
            return false;

        return true;
    }
}