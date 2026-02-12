using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class fireflyScript : MonoBehaviour
{
    public Vector2 velocity;
    [SerializeField] public float intensity;
    public double xPos;
    public double yPos;
    [SerializeField] public float lifetime;
    public LaunchScript l;
    void Start()
    {
        l = GameObject.FindGameObjectWithTag("Launcher").GetComponent<LaunchScript>();
        lifetime = l.flyCooldown;
        intensity = 2;
        xPos = this.transform.position.x;
    }
    void Update()
    {
        if(lifetime <=0)
        {
            Destroy(gameObject);
        }
        else
        {
            lifetime -= Time.deltaTime;
            xPos += velocity.x * Time.deltaTime;
            yPos = intensity * Math.Sin(xPos) + 5;
            this.transform.position = new Vector2((float) xPos,(float) yPos);
        }
    }
   void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "wall")
        {
            Debug.Log("ehehehe");
            Destroy(this.gameObject);
        }
    }
}
