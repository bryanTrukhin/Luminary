using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float damage;
    [SerializeField] protected float speed;
    [SerializeField] protected float knockbackStrength;

    [SerializeField] protected Rigidbody2D rb;
    public Vector2 forwardDir;
    public bool canMove;

    protected virtual void Start()
    {
        canMove = true;
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

    }

    protected virtual void FixedUpdate()
    {
        if (canMove)
        {
            HandleMovement();
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    protected virtual void HandleMovement()
    {
        rb.velocity = forwardDir.normalized * speed;
    }

    protected void FlipX()
    {
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            canMove = false;

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Note: We use the move direction for knockback
                playerRb.AddForce(forwardDir * knockbackStrength, ForceMode2D.Impulse);
            }
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
        }
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            canMove = true;
        }
    }
}