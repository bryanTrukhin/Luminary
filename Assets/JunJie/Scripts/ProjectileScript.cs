using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProjectileScript : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float travelTime;

    [Header("On Impact")]
    [SerializeField] private GameObject explosionParticlePrefab;
    [SerializeField] private float lightFadeDuration;
    private Vector2 destination;
    private float speed;
    private Light2D light;
    private bool dead = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dead) return;
        transform.position = Vector2.MoveTowards(
        transform.position,
        destination,
        speed * Time.deltaTime
    );

    if (Vector2.Distance(transform.position, destination) < 0.05f)
        StartCoroutine(OnImpact());
    }
    public void Init(Vector2 target)
    {
        destination = target;
        float distance = Vector2.Distance(transform.position, destination);
        speed = distance / travelTime;
        light = GetComponentInChildren<Light2D>();
    }
    IEnumerator OnImpact()
    {
    dead = true;

    GetComponent<SpriteRenderer>().enabled = false;

    if (explosionParticlePrefab)
        Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
    yield return null;
    Destroy(gameObject);
    }

}
