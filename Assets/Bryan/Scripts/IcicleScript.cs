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
    [SerializeField] public Vector2 startSize = new Vector2(0.1f, 0.1f);
    [SerializeField] public Vector2 endSize = new Vector2(0.5f, 0.5f);
    [SerializeField] public GameObject shatterEffect;
    public bool canShatter;
    [SerializeField] public GameObject shatterNoise;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialFallSpeed = fallSpeed;
    }

    void OnEnable()
    {
        transform.localScale = startSize;
        fallTimer = 0;
        isFalling = false;
        fallSpeed = initialFallSpeed;
        canShatter = false;

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
            CheckIfHit();
        }
        else
        {
            float step = fallTimer / buildUpDuration;
            float xScale = Mathf.Lerp(startSize.x, endSize.x, step);
            float yScale = Mathf.Lerp(startSize.y, endSize.y, step);
            transform.localScale = new Vector2(xScale, yScale);
        }
    }

    void CheckIfHit()
    {
        Vector2 origin = transform.position;
        float rayLength = transform.localScale.y * 0.6f;
        Debug.DrawRay(origin, -transform.up * rayLength, Color.magenta);

        RaycastHit2D hit = Physics2D.Raycast(origin, -transform.up, rayLength, platform);
        if (hit.collider != null)
        {
            canShatter = true;
            DespawnIcicle();
        }
    }

    void CheckIfSpawnedInsidePlatform()
    {
        Vector2 checkPos = (Vector2)transform.position - (Vector2)transform.up * 0.2f;
        float checkRadius = 0.01f;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, checkRadius, platform);

        if (hit != null)
        {
            DespawnIcicle();
        }
    }

    public void DespawnIcicle()
    {
        if (shatterEffect != null && canShatter)
        {
            Instantiate(shatterEffect, transform.position, Quaternion.identity);
            if (shatterNoise != null)
            {
                GameObject noiseInstance = Instantiate(shatterNoise, transform.position, Quaternion.identity);
                AudioSource audioSource = noiseInstance.GetComponent<AudioSource>();

                if (audioSource != null && audioSource.clip != null)
                {
                    Destroy(noiseInstance, audioSource.clip.length);
                }
                else
                {
                    Destroy(noiseInstance, 2f);
                }
            }
        }
        if (AntMoving.iciclePos.Contains(gameObject))
        {
            AntMoving.iciclePos.Remove(gameObject);
        }
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}