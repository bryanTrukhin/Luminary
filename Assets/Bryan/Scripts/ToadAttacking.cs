using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToadAttacking : MonoBehaviour
{
    [Header("General")]
    [SerializeField] public float detectionRadius;
    public ToadMovementVersion2 movementScript;
   

    [Header("Target Detection")]
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public LayerMask fireflyLayer;
    [SerializeField] public LayerMask obstacleLayer;
    public GameObject target;
    public Vector2 localTargetPos;
    public Vector2 lockedTargetPos;
    public bool foundTarget;
    public bool notBehindObstacle; //this variable exists soley for Gizmo visualization, NOT IMPORTANT
    private Vector2 tongueStartPos;

    [Header("Tongue Handling")]
    [SerializeField] public GameObject tongue;
    public DistanceJoint2D tongueJoint;
    public float reachSpeed;
    private float lerpTimer = 0f;
    public float tongueDuration;
    public bool canTongueGrab;
    private bool isRetracting = false;

    [Header("Death Handling")]
    [SerializeField] public GameObject lightReleaseEffect;
    public bool canExplode;


    void Start()
    {
        movementScript = GetComponent<ToadMovementVersion2>();
        tongueJoint = tongue.GetComponent<DistanceJoint2D>();
        tongueJoint.connectedAnchor = transform.position;
        tongueJoint.enabled = false;

        tongue.transform.SetParent(null);
        tongue.SetActive(false);

        Collider2D toadCollider = GetComponent<Collider2D>();
        Collider2D tongueCollider = tongue.GetComponent<Collider2D>();

        if (toadCollider != null && tongueCollider != null)
        {
            Physics2D.IgnoreCollision(toadCollider, tongueCollider, true);
        }

        canExplode = false;
        foundTarget = false;
        notBehindObstacle = false;
    }

    void Update()
    {   
        //Finding whether the player or a firefly entered the radius, with priority to the firefly
        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        Collider2D fireflyHit = Physics2D.OverlapCircle(transform.position, detectionRadius, fireflyLayer);

        if (fireflyHit != null)
        {
            target = fireflyHit.gameObject;
            foundTarget = true;
        }
        else if (playerHit != null)
        {
            target = playerHit.gameObject;
            foundTarget = true;
        }
        else
        {
            target = null;
            foundTarget = false;
            notBehindObstacle = false;

        }

        //Determining whether theres a obstacle between the toad and the target
        if (foundTarget)
        {
            Vector3 direction = target.transform.position - transform.position;
            RaycastHit2D obstacleTouched = Physics2D.Raycast(transform.position, direction.normalized, detectionRadius, obstacleLayer);
            if (!obstacleTouched)
            {
                localTargetPos = transform.InverseTransformPoint(Vector3Int.RoundToInt(target.transform.position));
                notBehindObstacle = true;
                canTongueGrab = true;
            }
            else
            {
                notBehindObstacle = false;
                canTongueGrab = false;
            }
        }

        if (canExplode)
        {
            selfDestruct();
        }
    }

    void FixedUpdate()
    {
        if (canTongueGrab && movementScript.canJump)
        {
            tongue.SetActive(true);
            TongueGrab();
        }
        if (!canTongueGrab)
        {
            tongue.SetActive(false);
            tongueJoint.connectedAnchor = transform.position;
        }

    }

    void TongueGrab()
    {
        tongueJoint.enabled = true;
        tongueJoint.autoConfigureConnectedAnchor = false;
        if (lerpTimer == 0f && !isRetracting)
        {
            if (target == null)
            {
                ResetTongue();
                return;
            }
            lockedTargetPos = Vector2Int.RoundToInt(target.transform.position);
        }
        lerpTimer += Time.deltaTime * reachSpeed;

        if (!isRetracting)
        {
            tongueJoint.connectedAnchor = Vector2.Lerp(tongueJoint.connectedAnchor, lockedTargetPos, lerpTimer);
            if (lerpTimer >= 1f)
            {
                lerpTimer = 0f;
                isRetracting = true;
            }
        }
        else
        {
            tongueJoint.connectedAnchor = Vector2.Lerp(tongueJoint.connectedAnchor, transform.position, lerpTimer);
            if (lerpTimer >= 1f)
            {
                ResetTongue();
            }
        }
    }

    void ResetTongue()
    {
        canTongueGrab = false;
        isRetracting = false;
        lerpTimer = 0f;
        tongueJoint.enabled = false;
        tongueJoint.connectedAnchor = transform.position;
    }

    void selfDestruct()
    {
        //1.) Create delay timer to play for the toad to have time to be on the ground before death
        //2.) Play animation of toad getting filled up like a balloon before popping

        Instantiate(lightReleaseEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
        //3.) Release a small batch of fireflies for player to collect
    }

    void OnDrawGizmos()
    {
        if (foundTarget && notBehindObstacle)
        {
            Gizmos.color = Color.red;
        }
        else if (foundTarget)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        if (foundTarget && target != null)
        {
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }
}