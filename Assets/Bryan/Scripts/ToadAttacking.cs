using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToadAttacking : MonoBehaviour
{
    [Header("General")]
    [SerializeField] public float detectionRadius;
    public bool canExplode;

    [Header("Target Detection")]
    [SerializeField] public LayerMask playerLayer;
    [SerializeField] public LayerMask fireflyLayer;
    [SerializeField] public LayerMask obstacleLayer;
    public GameObject target;
    public Vector2 localTargetPos;
    public Vector2 lockedTargetPos;
    public bool foundTarget;
    public bool notBehindObstacle; //this variable exists soley for Gizmo visualization, NOT IMPORTANT

    [Header("Tongue Handling")]
    public float reachSpeed;
    public GameObject tongue;
    private float lerpTimer = 0f;
    public float tongueDuration;
    public DistanceJoint2D tongueJoint;
    public bool canTongueGrab;
    private bool isRetracting = false;

    private bool facingRight = true;


    void Start()
    {
        tongueJoint = tongue.GetComponent<DistanceJoint2D>();
        tongueJoint.connectedAnchor = transform.position;
        canExplode = false;
        tongueJoint.enabled = false;
        foundTarget = false;
        notBehindObstacle = false;
    }

    void Update()
    {   
        //Finding whether the player or a firefly entered the radius
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
            UpdateFacingDirection();

            Vector3 direction = target.transform.position - transform.position;
            RaycastHit2D obstacleTouched = Physics2D.Raycast(transform.position, direction.normalized, detectionRadius, obstacleLayer);
            if (!obstacleTouched)
            {
                localTargetPos = transform.InverseTransformPoint(target.transform.position);
                notBehindObstacle = true;
                UpdateFacingDirection();
                canTongueGrab = true;
                //SET UP A COOLDOWN FOR WHEN THE TONGUE GRAB CAN BE CALLED AGAIN
            }
            else
            {
                notBehindObstacle = false;
            }
        }

        if (canExplode)
        {
            selfDestruct();
        }
    }

    void FixedUpdate()
    {
        if (canTongueGrab)
        {
            TongueGrab();
        }

    }

    void TongueGrab()
    {
        
        tongueJoint.enabled = true;
        tongueJoint.autoConfigureConnectedAnchor = false;
        if (lerpTimer == 0f && !isRetracting)
        {
            lockedTargetPos = target.transform.position;
        }
        lerpTimer += Time.deltaTime * reachSpeed;

        if (!isRetracting)
        {
            tongueJoint.connectedAnchor = Vector2.Lerp(tongueJoint.connectedAnchor, lockedTargetPos, lerpTimer);

            
            if (lerpTimer >= 1f)
            {
                lerpTimer = 0f;
                isRetracting = true;
                Debug.Log("Switching to Retract Mode");
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

    void UpdateFacingDirection()
    {
        if (target == null) return;

        float direction = target.transform.position.x - transform.position.x;

        if (direction > 0 && !facingRight)
        {
            Flip(true);
        }
        else if (direction < 0 && facingRight)
        {
            Flip(false);
        }
    }

    void Flip(bool faceRight)
    {
        facingRight = faceRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1 : -1);
        transform.localScale = scale;
        Debug.Log("Swapped!");

    }


    void selfDestruct()
    {
        Destroy(gameObject);
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