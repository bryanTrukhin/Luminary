using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class Dog_Script : MonoBehaviour
{
    public Rigidbody2D thisBody;
    public int refForce;
    public int facing;
    public float maxDown;
    public float maxOut;
    public Collider2D thisCollider;
    public Transform thisTransform;
    public Vector2 bounds;
    // defines "floor"
    // Start is called before the first frame update
    void Start()
    {
        refForce = 5;
        thisBody = this.GetComponent<Rigidbody2D>();
        thisCollider = this.GetComponent<BoxCollider2D>();
        thisTransform = this.GetComponent<Transform>();
        bounds = thisCollider.bounds.extents;
        maxDown = bounds.y + 1f;
        Debug.Log(maxDown);
        maxOut = bounds.x +2f;
        facing = 1;

        // testLine();
    }

    // Update is called once per frame
    void Update()
    {
        // linetest();
        // turnCheck();
        move();
        linetest();
    }
    private void move()
    {
        if (turnCheck())
        {
            facing*=-1;
            thisTransform.Rotate(0,180,0);
            Debug.Log("turned");
            thisBody.velocity = Vector3.zero;
            thisBody.angularVelocity = 0f;
        }
        Vector2 force = new Vector2(facing*refForce,0);
        thisBody.AddForce(force);
    }
    private void linetest()
    {
        // Vector3 check = this.transform.position + new Vector3(facing*bounds.x,0,0);
        // Debug.DrawRay(check,-this.transform.up.normalized *maxDown,Color.blue);
        // Debug.DrawRay(this.transform.position,facing*this.transform.right.normalized*maxOut,Color.green);
        Vector3 forwardCheck = facing*this.transform.right.normalized;
        Debug.DrawRay(this.transform.position,forwardCheck*maxOut,Color.blue);
    }
    private bool turnCheck()
    {
        bool hitting = false;
        Vector3 downCheck = this.transform.position + new Vector3(facing*bounds.x,0,0);
        Vector3 forwardCheck = (facing > 0) ? Vector3.right:Vector3.left;
        Debug.DrawRay(this.transform.position,forwardCheck*maxOut,Color.blue);
        RaycastHit2D dHit = Physics2D.Raycast(downCheck,-this.transform.up.normalized,maxDown); 
        RaycastHit2D fHit = Physics2D.Raycast(this.transform.position,forwardCheck,maxOut);
        if (dHit.collider == null)
        {
            hitting = true;
        }
        else if (fHit.collider != null)
        {
            hitting = true; 
        }
        else if (dHit.collider != null)
        {
            hitting = false;
        }
        return hitting;
    }
}
