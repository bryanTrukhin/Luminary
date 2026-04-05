using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueRendering : MonoBehaviour
{
    public GameObject toad;
    public ToadAttacking toadAttacking;
    public LineRenderer lineRenderer;
    public CircleCollider2D circleCollider;

    // Start is called before the first frame update
    void Start()
    {
        toadAttacking = toad.GetComponent<ToadAttacking>();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = toad.transform.position;
        circleCollider.offset = transform.InverseTransformPoint(toadAttacking.tongueJoint.connectedAnchor);
        lineRenderer.SetPosition(0, toadAttacking.transform.position);
        lineRenderer.SetPosition(1, toadAttacking.tongueJoint.connectedAnchor);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
        }
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("PLAYER WAS HIT");
        }
        if (collision.gameObject.CompareTag("Firefly"))
        {
            Debug.Log("FIREFLY WAS HIT");
            toadAttacking.canExplode = true;
            Destroy(collision.gameObject);
        }
    }
}