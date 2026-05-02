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
        if (isAttacking) return;

        float speed = GetCurrentSpeed();

        Move(speed);
        CheckWall();
        CheckEdge();

    }


    protected virtual void Update()
    {
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
            facingDir = player.position.x > transform.position.x ? 1 : -1;
            return chaseSpeed;
        }

        return patrolSpeed;
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


}
