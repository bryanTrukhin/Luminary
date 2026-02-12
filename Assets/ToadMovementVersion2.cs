using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool canJump;

    // Gizmo variable to track the actual point we are aiming for
    private Vector3 debugLandingPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (gameManager != null)
        {
            nav = gameManager.GetComponent<NavGraphBuilder>();
        }
        canJump = false;
    }

    void Update()
    {
        currentPos = transform.position;
        if (nav != null && nav.debugTilePath != null)
        {
            if (path != nav.debugTilePath)
            {
                path = nav.debugTilePath;
                pathIndex = 0;
            }
        }

        if (path != null && pathIndex < path.Count - 1 && canJump)
        {
            ConductJump();
        }
    }

    void ConductJump()
    {
        canJump = false;

        int step = (pathIndex + 2 < path.Count) ? 2 : 1;
        pathIndex += step;

        Vector2 targetPos = path[pathIndex].worldPos;

        // We set the debug position here so it updates every jump
        debugLandingPos = new Vector3(targetPos.x, targetPos.y + 0.5f, 0);

        float height = (step == 2) ? jumpHeightConstant * 1.5f : jumpHeightConstant;
        rb.velocity = CalculateLaunchVelocity(currentPos, targetPos, height);

        Debug.Log($"Jumped {step} tiles! Now at path index: {pathIndex}");
    }

    public Vector2 CalculateLaunchVelocity(Vector3 start, Vector3 target, float jumpHeight)
    {
        float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        // Adjust for the surface of the tile
        float targetSurfaceY = target.y + 0.5f;

        float dispX = (target.x - start.x);
        float dispY = targetSurfaceY - start.y;

        // 2. DEFINE PEAK (Fixed for flat ground/downward jumps)
        // Using Mathf.Max ensures the peak is always higher than both start and target
        float worldPeak = Mathf.Max(start.y, targetSurfaceY) + jumpHeight;

        // Use Abs to prevent NaN (Square root of negative) from floating point errors
        float h1 = Mathf.Abs(worldPeak - start.y);
        float h2 = Mathf.Abs(worldPeak - targetSurfaceY);

        // 3. TIME
        float timeUp = Mathf.Sqrt(2 * h1 / g);
        float timeDown = Mathf.Sqrt(2 * h2 / g);
        float totalTime = timeUp + timeDown;

        // Prevent Velocity Explosion on flat ground
        if (totalTime < 0.01f) totalTime = 0.1f;

        // 4. VELOCITIES
        float vX = dispX / totalTime;
        float vY = Mathf.Sqrt(2 * g * h1);

        return new Vector2(vX, vY);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            if (rb.velocity.y <= 0.1f)
            {
                canJump = true;
            }
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