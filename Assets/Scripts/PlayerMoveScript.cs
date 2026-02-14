using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;


// using System.Numerics;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour
{
    public int facing;
    public Transform player;
    public Vector3 moveSpeed;
    public Vector3 jumpForce;
    public Vector3 dashForce;
    public Rigidbody2D playerMove;
    public bool jumping;
    public bool dashing; 
    public float dashtimer;
    public float dashTime;
    public Transform facingRight;
    public Transform facingLeft;
    
    // Start is called before the first frame update
    void Start()
    {
        // the direction the player is facing,  1 is right, -1 is left
        facingRight = GameObject.Find("facing").GetComponent<Transform>();
        facing = 1;
        player = this.GetComponent<Transform>();
        playerMove = this.GetComponent<Rigidbody2D>();
        jumping = false;
        dashTime = 5f;
        dashing = false;
        dashtimer = dashTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            move(-1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            move(1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if(!jumping){
                jump();
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(!dashing){
                dash();
            }
        }
        if (dashing)
        {
            dashTimer();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            crawl();
        }
    }
    public void move(int dir)
    {
        if( dir != facing)
        {
            // change facing
            player.Rotate(0,180,0);
            dashForce.x = -dashForce.x; 
            facing = -facing;
        }
        if(dir == -1)
        {
            playerMove.AddForce(-moveSpeed);
        }
        else if (dir == 1)
        {
            playerMove.AddForce(moveSpeed);
        }
    }
    public void jump()
    {
        playerMove.AddForce(jumpForce,ForceMode2D.Impulse);
        jumping = true; 
    }
    public void dash()
    {
        playerMove.AddForce(dashForce,ForceMode2D.Impulse);
        dashing = true;  
    }
    public void dashTimer()
    {
        if(dashtimer <= 0)
        {
            dashing = false;
            dashtimer = dashTime;
            return;
        }
        else{
            dashtimer -= Time.deltaTime;
        }
    }
    public void crawl()
    {
        player.Rotate(0,0,90);
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "floor")
        {
            jumping = false;
        }
    }
}
