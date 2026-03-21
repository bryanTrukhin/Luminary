using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Linq;

public class ToadMovementVersion2 : MonoBehaviour
{
    [Header("General")]
    public Rigidbody2D rb;
    public Vector2 currentPos;

    [Header("Pathfinding")]
    public GameObject gameManager;
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

    // Gizmo variable to track the actual point we are aiming for
    private Vector3 debugLandingPos;

    public Transform player;
    [HideInInspector] public bool isAttacking = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (gameManager != null)
        {
            nav = gameManager.GetComponent<NavGraphBuilder>();
            Grid grid = tilemap.layoutGrid;
            tileOffset= grid.cellSize.y / 2f;
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        canJump = false;
        isJumping = false;
    }

    void Update()
    {
        UpdateFacing();

        if (isAttacking) return;

        currentPos = transform.position;
        if (nav != null && nav.debugTilePath != null)
        {
            if (path != nav.debugTilePath)
            {
                path = nav.debugTilePath;
                pathIndex = 0;
            }
        }

        if (path != null && pathIndex < path.Count - 1 && canJump && !isJumping)
        {
            StartCoroutine(JumpSequence());
        }
    }

    void UpdateFacing()
    {
        if (player == null) return;

        float direction = player.position.x - transform.position.x;

        Vector3 scale = transform.localScale;

        if (direction > 0)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else if (direction < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }


    Vector2 targetPos;
    IEnumerator JumpSequence()
    {
        canJump = false;
        isJumping = true;

        int step = 1;
        targetPos = path[pathIndex + step].worldPos;
        Vector2 homeTilePos = path[pathIndex].worldPos;

        //Re-adjusting
        float distToTarget = Mathf.Abs(transform.position.x - targetPos.x);
        if (distToTarget < tileOffset * 2f)
        {
            rb.velocity = CalculateLaunchVelocity(transform.position, homeTilePos, jumpHeightConstant * 0.5f);
            yield return new WaitForSeconds(0.5f);
        }

        //Main jump
        pathIndex += step;
        debugLandingPos = new Vector3(targetPos.x, targetPos.y + tileOffset, 0);
        //Debug.Log($"Leaping {step} tiles!");

        rb.velocity = CalculateLaunchVelocity(transform.position, targetPos, jumpHeightConstant);
        yield return new WaitUntil(() => canJump);
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

    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //RaycastHit2D downCheck = Physics2D.Raycast(currentPos, Vector2.down, tileOffset, platformLayerMask);
        if (collision.gameObject.CompareTag("Platform"))
        {
            canJump = true;
        }
    }
    */

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
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