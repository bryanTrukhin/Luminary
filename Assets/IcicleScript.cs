using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleScript : MonoBehaviour
{
    public AntMoving antScript;
    public Rigidbody2D rb;
    [SerializeField] public float buildUpDuration;
    [SerializeField] public float fallSpeed;
    [SerializeField] public float fallAcceleration = 0.5f;
    private float initialFallSpeed;
    public float fallTimer;
    public bool isFalling;
    [SerializeField] public LayerMask platform;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialFallSpeed = fallSpeed;
    }

    void OnEnable()
    {
        fallTimer = 0;
        isFalling = false;
        fallSpeed = initialFallSpeed;

        if (rb != null)
        {
            rb.simulated = false;
            rb.velocity = Vector2.zero;
        }
        CheckIfSpawnedInsidePlatform();
        
    }

    void FixedUpdate()
    {
        fallTimer += Time.fixedDeltaTime;
        if (fallTimer >= buildUpDuration)
        {
            if (!isFalling)
            {
                rb.simulated = true;
                isFalling = true;
            }

            rb.velocity = -transform.up * fallSpeed;
            fallSpeed += fallAcceleration;
        }
        CheckIfHit();
    }

    void CheckIfHit()
    {
        Vector2 origin = transform.position;
        float rayLength = transform.localScale.y * 0.6f;
        Debug.DrawRay(origin, -transform.up * rayLength, Color.magenta);

        RaycastHit2D hit = Physics2D.Raycast(origin, -transform.up, rayLength, platform);

        if (hit.collider != null)
        {
            DespawnIcicle();
        }
    }
    
    void CheckIfSpawnedInsidePlatform()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D hit = Physics2D.OverlapBox(transform.position, myCollider.bounds.size, 0f);
        if (hit != null && hit.CompareTag("Platform"))
        {
            DespawnIcicle();
        }
    }

    public void DespawnIcicle()
    {
        if (AntMoving.iciclePos.Contains(gameObject))
        {
            AntMoving.iciclePos.Remove(gameObject);
        }
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}