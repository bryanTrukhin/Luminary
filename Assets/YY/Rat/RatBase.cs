using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RatBase : MonoBehaviour
{
    [Header("General")]
    public float health = 10f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public int facingDir = 1;

    [Header("Detection")]
    public LayerMask platform;
    public float wallCheckDist = 0.4f;
    public float edgeCheckDist = 0.7f;

    [Header("Player")]
    public Transform player;
    public float detectionDistance = 6f;
    public float samePlatformHeight = 0.8f;

    protected Rigidbody2D rb;
    protected bool isAttacking = false;

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.25f;

    protected bool isKnockedBack = false;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Awake()
    {
        FindPlayer();
    }


    protected virtual void FixedUpdate()
    {
        if (isAttacking || isKnockedBack)
        {
            return;
        }
        float speed = GetCurrentSpeed();

        Move(speed);
        CheckWall();
        CheckEdge();

    }


    protected virtual void Update()
    {
        if (isKnockedBack)
            return;

        TryAttack();
        CheckPlayerExists();
    }

    protected float GetCurrentSpeed()
    {
        if (player == null) return patrolSpeed;

        float dist = Vector2.Distance(transform.position, player.position);
        float heightDiff = Mathf.Abs(player.position.y - transform.position.y);

        if (dist < detectionDistance && heightDiff < samePlatformHeight)
        {
            float xDiff = player.position.x - transform.position.x;

            if (Mathf.Abs(xDiff) > 0.3f)
            {
                int targetDir = xDiff > 0 ? 1 : -1;

                if (targetDir != facingDir)
                {
                    Flip();
                }
            }

            return chaseSpeed;
        }

        return patrolSpeed;
    }

    protected void FacePlayer()
    {
        if (player == null) return;

        float xDiff = player.position.x - transform.position.x;

        int targetDir = xDiff > 0 ? 1 : -1;

        if (targetDir != facingDir)
        {
            Flip();
        }
    }

    protected virtual void Move(float speed)
    {
        rb.velocity = new Vector2(facingDir * speed, rb.velocity.y);
    }

    protected void CheckWall()
    {
        Vector2 dir = Vector2.right * facingDir;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, wallCheckDist, platform);

        if (hit.collider != null)
            Flip();
    }

    protected void CheckEdge()
    {
        Vector2 origin = transform.position + new Vector3(0.4f * facingDir, 0);
        RaycastHit2D ground = Physics2D.Raycast(origin, Vector2.down, edgeCheckDist, platform);

        if (ground.collider == null)
            Flip();
    }

    protected void Flip()
    {
        facingDir *= -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDir;
        transform.localScale = scale;
    }

    protected abstract void TryAttack();

    protected void FindPlayer()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");

        if (obj != null)
            player = obj.transform;
    }
    protected void CheckPlayerExists()
    {
        if (player == null)
        {
            FindPlayer();
        }
    }

    public virtual void TakeDamage(float damage, Vector2 hitSource)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(KnockbackRoutine(hitSource));
    }

    IEnumerator KnockbackRoutine(Vector2 hitSource)
    {
        isKnockedBack = true;

        rb.velocity = Vector2.zero;

        Vector2 direction = ((Vector2)transform.position - hitSource).normalized;

        direction += Vector2.up * 0.5f;

        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
    }
    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
