using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueRendering : MonoBehaviour
{
    public GameObject toad;
    public ToadAttacking toadAttacking;
    public LineRenderer lineRenderer;
    public CircleCollider2D circleCollider;

    public bool isSticking;
    public Transform currentTarget;

    // Start is called before the first frame update
    void Start()
    {
        toadAttacking = toad.GetComponent<ToadAttacking>();
        circleCollider = GetComponent<CircleCollider2D>();
        isSticking = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = toad.transform.position;
        circleCollider.offset = transform.InverseTransformPoint(toadAttacking.tongueJoint.connectedAnchor);
        lineRenderer.SetPosition(0, toadAttacking.transform.position);
        lineRenderer.SetPosition(1, toadAttacking.tongueJoint.connectedAnchor);
    }

    private void FixedUpdate()
    {
        if (isSticking && currentTarget != null)
        {
            currentTarget.position = toadAttacking.tongueJoint.connectedAnchor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("PLAYER WAS HIT");
            currentTarget = collision.gameObject.transform;
            isSticking = true;
        }
        else
        {
            isSticking = false;
        }

        if (collision.gameObject.CompareTag("Firefly"))
        {
            Debug.Log("FIREFLY WAS HIT");
            toadAttacking.canExplode = true;
            Destroy(collision.gameObject);
        }
    }
}