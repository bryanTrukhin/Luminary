using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class AntMoving : EnemyController /*MonoBehaviour*/
{
    [Header("General")]
    public int facingDir = 1;
    //public Vector2 forwardDir;
   // public Rigidbody2D rb;
    //public float speed = 1f;
    public float gravStrength = 2f;
    public bool canFlip = true;

    [Header("Rotating")]
    [SerializeField] public LayerMask platformLayer;
    [SerializeField] public LayerMask enemyLayer;
    public Vector3 lastValidLedge;
    public bool isTurning;

    [Header("Trail Creation")]
    [SerializeField] public GameObject iceSheet;
    [SerializeField] public GameObject icicle;
    [SerializeField] public int spaceBetweenIcicles = 2;
    public float icicleSpawnTimer = 0;
    public bool canSpawnIceSheets;
    public Vector2 lastKnownCornerPos;
    public Vector2 lastKnownIciclePos;
    [SerializeField] public LayerMask icicleLayer;
    public static List<GameObject> iciclePos = new List<GameObject>();

    [Header("Effects")]
    [SerializeField] public GameObject vaporEffect;

    protected override void Start()
    {
        base.Start();
        //rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        isTurning = false;
        canSpawnIceSheets = false;
    }

    protected override void HandleMovement()
    {
        if (isTurning) return;

        facingDir = (transform.localScale.x < 0) ? -1 : 1; 
        Vector2 moveDir = (Vector2)transform.right * facingDir;
        forwardDir = moveDir;

        //Setting overhang raycast to be relative to box collider,not transform scale
        //RaycastHit2D overhangCheck = Physics2D.Raycast(transform.position, -transform.up, transform.localScale.y * 2f, platform);
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        float distanceToFeet = col.size.y * 0.5f;
        float rayLength = distanceToFeet + 0.2f;
        RaycastHit2D overhangCheck = Physics2D.Raycast(transform.position, -transform.up, rayLength, platformLayer);
        Debug.DrawRay(transform.position, -transform.up * transform.localScale.y * 2f, Color.yellow);

        if (overhangCheck.collider != null)
        {
            lastValidLedge = overhangCheck.point;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            //Adding a buffer in case the ant gets stuck randomly
            if (canMove && rb.velocity.sqrMagnitude < 0.1f)
            {
                float pushAmount = 0.01f * facingDir;
                transform.position += new Vector3(pushAmount, 0, 0);
            }
            rb.velocity = moveDir * speed;
            rb.AddForce(-transform.up * gravStrength);
            canFlip = true;
        }
        else
        {
            StartCoroutine(RotateAroundPivot(lastValidLedge));
        }

        FlippingCheck(moveDir);
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
        lastKnownCornerPos = transform.position;
    }

    void FlippingCheck(Vector2 moveDir)
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        float colliderWidth = myCollider.bounds.extents.x;
        float rayDist = colliderWidth + 0.1f;

        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, moveDir, rayDist, platformLayer);
        Debug.DrawRay(transform.position, moveDir * rayDist, Color.red);
        if (wallHit.collider != null)
        {
            FlipX();
        }
    }

    void CreateTrail()
    {
        //Setting up spawn position
        Vector3 spawnLocation = transform.position;
        Collider2D col = GetComponent<Collider2D>();

        float feetOffset = col.bounds.extents.y;
        float backOffset = col.bounds.extents.x; //0.5f
        spawnLocation -= transform.right * facingDir * backOffset;
        spawnLocation -= transform.up * feetOffset;

        //Setting up constraints
        float currentZ = transform.eulerAngles.z;
        float distanceFromCorner = Vector2.Distance(transform.position, lastKnownCornerPos);

        //ICE MANAGEMENT
        if (Mathf.Approximately(currentZ, 0f) && canSpawnIceSheets && distanceFromCorner >= 1f)
        {
            ObjectPoolManager.SpawnObject(iceSheet, spawnLocation, Quaternion.identity, ObjectPoolManager.PoolType.Gameobject);
        }

        //ICICLE MANAGEMENT
        if (Mathf.Approximately(currentZ, 180f))
        {
            float distanceFromLast = (iciclePos.Count > 0)
                ? Vector2.Distance(spawnLocation, iciclePos[iciclePos.Count - 1].transform.position)
                : float.MaxValue;

            bool isStickingToOldTrail = false;
            foreach (GameObject oldPos in iciclePos)
            {
                if (Vector2.Distance(spawnLocation, oldPos.transform.position) < spaceBetweenIcicles)
                {
                    isStickingToOldTrail = true;
                    break;
                }
            }

            if (distanceFromCorner >= 1 && distanceFromLast >= spaceBetweenIcicles && !isStickingToOldTrail)
            {
                GameObject newIcicle = ObjectPoolManager.SpawnObject(icicle, spawnLocation, Quaternion.identity, ObjectPoolManager.PoolType.Gameobject);
                newIcicle.GetComponent<IcicleScript>().antScript = this;
                iciclePos.Add(newIcicle);
            }
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if (collision.gameObject.CompareTag("Platform"))
        {
            canSpawnIceSheets = true;
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Fire"))
        {
            Instantiate(vaporEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        //Clearing the static array memory on death
        if (iciclePos != null)
        {
            iciclePos.Clear();
        }
    }
}