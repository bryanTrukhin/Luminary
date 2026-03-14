using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class AntMoving : MonoBehaviour
{
    [Header("General")]
    public int facingDir = 1;
    public Vector2 forwardDir;
    public Rigidbody2D rb;
    public float speed = 1f;
    public float gravStrength = 2f;
    public bool canFlip = true;

    [Header("Rotating")]
    [SerializeField] public LayerMask platform;
    public Vector3 lastValidLedge;
    public bool isTurning;

    [Header("Trail Creation")]
    [SerializeField] public GameObject iceSheet;
    [SerializeField] public GameObject icicle;
    public float icicleSpawnTimer = 0;
    public bool canSpawnIceSheets;
    public bool canSpawnIcicles;
    public Vector2 lastKnownPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        isTurning = false;
        canSpawnIceSheets = false;
        canSpawnIcicles = true;
    }

    void FixedUpdate()
    {
        if (isTurning) return;
        facingDir = (transform.localScale.x < 0) ? -1 : 1;
        Vector2 moveDir = transform.right * facingDir;

        RaycastHit2D overhangCheck = Physics2D.Raycast(transform.position, -transform.up, transform.localScale.y * 2f, platform);
        if (overhangCheck.collider != null)
        {
            lastValidLedge = overhangCheck.point;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.velocity = moveDir * speed;
            rb.AddForce(-transform.up * gravStrength);
            canFlip = true;
        }
        else
        {
            StartCoroutine(RotateAroundPivot(lastValidLedge));
        }

        WallCheck(moveDir);
        CreateTrail();
    }

    IEnumerator RotateAroundPivot(Vector3 pivot)
    {
        isTurning = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.constraints = RigidbodyConstraints2D.None;

        float timer = 0;
        float duration = 0.5f;

        Quaternion startRot = transform.rotation;
        Vector3 startOffset = transform.position - pivot;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, -90f * facingDir);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float percent = timer / duration;
            Quaternion currentRot = Quaternion.Slerp(startRot, targetRot, percent);
            rb.MoveRotation(currentRot);

            //This effectively updates your currentRot to account for the change from the startRot
            //This way everything is not just based on startRot
            Quaternion relativeChange = currentRot * Quaternion.Inverse(startRot);
            rb.MovePosition(pivot + (relativeChange * startOffset));
            yield return null;
        }
        transform.rotation = targetRot;
        isTurning = false;
        lastKnownPos = transform.position;
    }

    void WallCheck(Vector2 moveDir)
    {
        float rayDist = Mathf.Abs(transform.localScale.x/1.5f);
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDir, rayDist, platform);
        Debug.DrawRay(transform.position, moveDir * rayDist, Color.red);
        if (wallHit.collider != null)
        {
            float dot = Vector2.Dot(wallHit.normal, moveDir);
            if (dot < 0f)
            {
                Vector3 newScale = transform.localScale;
                newScale.x *= -1;
                transform.localScale = newScale;
            }
        }
    }

    void CreateTrail()
    {
        //Setting up spawn position
        Vector3 spawnLocation = transform.position;
        float backOffset = 0.5f;
        spawnLocation -= transform.right * facingDir * backOffset;
        float feetOffset = transform.localScale.y * 0.5f;
        spawnLocation -= transform.up * feetOffset;

        //Setting up constraints
        float currentZ = transform.eulerAngles.z;
        float distance = Vector2.Distance(transform.position, lastKnownPos);

        if (Mathf.Approximately(currentZ, 0f) && canSpawnIceSheets && distance >= 1f)
        {
            ObjectPoolManager.SpawnObject(iceSheet, spawnLocation, Quaternion.identity, ObjectPoolManager.PoolType.Gameobject);
            //lastKnownPos = transform.position;
        }

        if (Mathf.Approximately(currentZ, 180f))
        {
            icicleSpawnTimer += Time.deltaTime;
            if (icicleSpawnTimer * speed >= 1f /*&& canSpawnIcicles*/) //we make it 1f b/c that is the length of 1 unity unit
            {
                ObjectPoolManager.SpawnObject(icicle, spawnLocation, Quaternion.identity, ObjectPoolManager.PoolType.Gameobject);
                icicleSpawnTimer = 0;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            canSpawnIceSheets = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        canSpawnIcicles = collision.gameObject.CompareTag("Icicle") ? false : true;
    }
}