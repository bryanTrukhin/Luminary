using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToadMovement : MonoBehaviour
{
    [Header("General")]
    public Rigidbody2D rb;
    public Vector2 currentPos;
    [SerializeField] public GameObject targetHandle;
    [SerializeField] public Vector2 targetPoint;

    [Header("Jump Handling")]
    [SerializeField] public float upwardForce;
    [SerializeField] public float forwardForce;
    public int smallJumpSize = 1; //1x1 units
    public int mediumJumpSize = 2; //2x2 units
    public int largeJumpSize = 4; //4x4 units
    public bool canJump;

    public int smallJumpCount;
    public bool didSmallJump;

    public int mediumJumpCount;
    public bool didMediumJump;

    [Header("Current Platform Data")]
    public GameObject currentPlatform;
    public int platformLength;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        upwardForce = 6f;
        forwardForce = 1f;
        didSmallJump = false;
        didMediumJump = false;
    }

    void Update()
    {
        currentPos = transform.position;
        targetPoint = targetHandle.transform.position; //this is just for testing, it can be changed when player tracking is implemented
        if (smallJumpCount + mediumJumpCount > 0 && canJump)
        {
            RegularLeap();
        }
    }

    public void RegularLeap()
    {
        canJump = false;
        int targetBias = 1;
        if (targetPoint.x < currentPos.x)
        {
            targetBias = -1;
        }

        rb.velocity = Vector2.zero;
        Vector2 jumpDirection = new Vector2(forwardForce * targetBias, upwardForce);
        if (mediumJumpCount > 0)
        {
            jumpDirection *= mediumJumpSize;
            mediumJumpCount--;
        }
        else
        {
            jumpDirection *= smallJumpSize;
            smallJumpCount--;
        }
        rb.AddForce(jumpDirection, ForceMode2D.Impulse);
        Debug.Log("Medium jumps remaining: " + mediumJumpCount);
        Debug.Log("Small jumps remaining: " + smallJumpCount);
    }


    //Create the TILEMAP FIRST
    /*
    public Vector2 FindGap()
    {
        List<Vector2> jumpPatterns = new List<Vector2>()
        {
            new Vector2(1, 0),  // Small Flat
            new Vector2(1, 1),  // Small Up
            new Vector2(2, 0),  // Medium Flat
            new Vector2(2, 2),  // Medium Up
            new Vector2(5, 0),  // Large Flat (Gap of 4 + 1 to land)
            new Vector2(5, 4),  // Large High (Gap 4, Height 4)
            new Vector2(5, -4)  // Large Drop
        };
    }
    */

    /*
    public void GapLeap()
    {

    }
    */

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            canJump = true;

            if (currentPlatform == null)
            {
                currentPlatform = collision.gameObject;
                Bounds platformBounds = collision.collider.bounds;
                float remainingDist;

                if (targetPoint.x > transform.position.x)
                {
                    // Moving Right: Distance to the Right Edge
                    remainingDist = platformBounds.max.x - transform.position.x;
                }
                else
                {
                    // Moving Left: Distance to the Left Edge
                    remainingDist = transform.position.x - platformBounds.min.x;
                }
                //remainingDist = Mathf.Max(0, remainingDist - 0.5f);
                int totalUnits = Mathf.FloorToInt(remainingDist);

                mediumJumpCount = totalUnits / mediumJumpSize;
                smallJumpCount = (totalUnits % mediumJumpSize) / smallJumpSize;

                Debug.Log($"Total units: {totalUnits}. Jumps: {mediumJumpCount} Med, {smallJumpCount} Small");
            }
        }
    }

    //dont worry about this yet, once the toad gets into the Bezier Curve jump, then you can alter the currentPlatform 
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == currentPlatform)
        {
            canJump = false;
        }
    }
    

}
