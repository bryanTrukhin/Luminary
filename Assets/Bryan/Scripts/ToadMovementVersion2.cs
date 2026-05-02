using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ToadMovementVersion2 : EnemyController
{
    [Header("General")]
    public bool canFlip;

    [Header("Pathfinding")]
    public NavGraphBuilder nav;
    public List<TileNode> path;
    public int pathIndex = 0;

    [Header("Jumping")]
    [SerializeField] public float jumpHeightConstant;
    [SerializeField] public Tilemap tilemap;
    [SerializeField] public LayerMask platformLayerMask;
    public float tileOffset;
    public bool canJump;
    public bool isJumping;

    [Header("Attacking")]
    public ToadAttacking attackingScript;
    private Vector3 debugLandingPos;

    protected override void Start()
    {
        base.Start();
        attackingScript = GetComponent<ToadAttacking>();
        StartCoroutine(WaitForGeneration());
    }

    IEnumerator WaitForGeneration()
    {
        yield return new WaitForSeconds(2);
        nav = GameObject.FindGameObjectsWithTag("Game Manager")[0].GetComponent<NavGraphBuilder>();
        yield return new WaitUntil(() => nav != null && nav.tilemap != null && nav.target != null);

        tilemap = nav.tilemap;
        tileOffset = tilemap.layoutGrid.cellSize.y / 2f;

        canJump = true;
        isJumping = false;
        canFlip = true;
    }

    protected override void FixedUpdate()
    {
        if (nav == null || tilemap == null || nav.target == null) return;

        if (canFlip)
        {
            Vector3 localTargetPos = transform.InverseTransformPoint(nav.target.position);
            if (localTargetPos.x < 0)
            {
                FlipX();
                float worldDirToPlayer = Mathf.Sign(nav.target.position.x - transform.position.x);
                forwardDir = new Vector2(worldDirToPlayer, 0);
                canFlip = false;
            }
        }

        if (canJump && !isJumping)
        {
            path = nav.GetTilePath(transform.position, nav.target.position);
            pathIndex = 0;

            if (path != null && path.Count > 1)
            {
                StartCoroutine(JumpSequence());
            }
        }
    }

    Vector2 targetPos;
    IEnumerator JumpSequence()
    {
        canJump = false;
        isJumping = true;

        int step = 1;
        targetPos = path[pathIndex + step].worldPos;
        Vector2 homeTilePos = path[pathIndex].worldPos;

        float xDistToTarget = transform.position.x - targetPos.x;
        float yDistToTarget = transform.position.y - targetPos.y;

        if (Mathf.Abs(xDistToTarget) < tileOffset * 2f && Mathf.Abs(yDistToTarget) > 0)
        {
            rb.velocity = CalculateLaunchVelocity(transform.position, homeTilePos, jumpHeightConstant * 0.5f);
            yield return new WaitForSeconds(0.5f);

            float toadHalfHeight = 0f;

            CapsuleCollider2D capCol = GetComponent<CapsuleCollider2D>();
            if (capCol != null) toadHalfHeight = capCol.bounds.extents.y;

            float surfaceY = homeTilePos.y + tileOffset + toadHalfHeight + 0.02f;
            Vector3 adjustLandingPos = new Vector3(homeTilePos.x, surfaceY, transform.position.z);
            transform.position = adjustLandingPos;
        }

        //Main jump
        pathIndex += step;
        debugLandingPos = new Vector3(targetPos.x, targetPos.y + tileOffset, 0);

        rb.velocity = CalculateLaunchVelocity(transform.position, targetPos, jumpHeightConstant);
        yield return new WaitUntil(() => canJump);
        canFlip = true;

        //Standing still until attacking is finished
        yield return new WaitUntil(() => !attackingScript.canTongueGrab);
        isJumping = false;
    }

    public Vector2 CalculateLaunchVelocity(Vector3 start, Vector3 target, float jumpHeight)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float displacementY = (target.y + tileOffset) - start.y;
        float displacementX = target.x - start.x;

        float heightFromStart = Mathf.Max(0, displacementY) + jumpHeight;
        float timeUp = Mathf.Sqrt((2 * heightFromStart) / gravity);
        float heightFromPeakToTarget = heightFromStart - displacementY;
        float timeDown = Mathf.Sqrt((2 * Mathf.Abs(heightFromPeakToTarget)) / gravity);
        float totalJumpTime = timeUp + timeDown;

        float velocityY = Mathf.Sqrt(2 * gravity * heightFromStart);
        float velocityX = displacementX / totalJumpTime;
 
        return new Vector2(velocityX, velocityY);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Platform"))
        {
            canJump = true;
        }
    }

    // DRAWING THE GIZMO
    private void OnDrawGizmos()
    {
        // 1. Draw a sphere at the target landing point
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(debugLandingPos, 0.3f);

        // 2. Draw a line from the Toad to the target to see the path
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, debugLandingPos);
        }

        // 3. Optional: Draw the entire path
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < path.Count; i++)
            {
                Gizmos.DrawSphere(path[i].worldPos, 0.1f);
            }
        }
    }
}