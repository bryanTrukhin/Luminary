using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSheetScript : MonoBehaviour
{
    [SerializeField] public float slipperyStrength = 5f;
    [SerializeField] public int lifetimeDuration = 10;
    [SerializeField] public float meltSpeed = 1f;
    public float meltTimer;

    public float currentOpacity;
    private SpriteRenderer spriteRenderer;
    private Color color;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        currentOpacity = 1f;
        meltTimer = 0f;

        if (spriteRenderer != null)
        {
            color = spriteRenderer.color;
            color.a = currentOpacity;
            spriteRenderer.color = color;
        }
    }

    void Update()
    {
        meltTimer += Time.deltaTime * meltSpeed;
        currentOpacity = 1f - (meltTimer / lifetimeDuration);
        if (spriteRenderer != null)
        {
            color.a = Mathf.Clamp01(currentOpacity);
            spriteRenderer.color = color;
        }

        if (meltTimer >= lifetimeDuration)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }


    //CONTINUE TO WORK ON THIS OR DO SOMTHING ELSE WHEN MAKING THE ICE SLIPPERY
    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            rb.angularDrag =
        }
    }
    */
}
