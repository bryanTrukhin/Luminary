using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Mail;
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
    public Vector3 crouching;
    public string priorOrient;
    public Rigidbody2D playerMove;
    public bool jumping;
    public bool dashing; 
    public float dashtimer;
    public float dashTime;
    public Transform facingTri;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // the direction the player is facing,  1 is right, -1 is left
        facingTri = GameObject.Find("facing").GetComponent<Transform>();
        facing = 1;
        player = this.GetComponent<Transform>();
        playerMove = this.GetComponent<Rigidbody2D>();
        jumping = false;
        dashTime = 5f;
        dashing = false;
        dashtimer = dashTime;
        crouching = new Vector3 (0,0,270);
    }

    // Update is called once per frame
    void Update()
    {   
        facingTri.position = new Vector3 (player.position.x,player.position.y,-1);
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
            crouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift)){
            getUp();
        }
    }
    public void move(int dir)
    {
        if( dir != facing)
        {
            // change facing
            // player.Rotate(0,180,0);

            // facingTri.rotation = Quaternion.FromToRotation();
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
    public void crouch()
    {
        if( player.rotation.y == 180)
        {
            priorOrient = "left";
        }
        else
        {
            priorOrient ="right";
        }
        player.rotation = Quaternion.Euler(crouching);
    }
    public void getUp()
    {

        if(priorOrient == "left")
        {
            player.rotation = Quaternion.Euler(0,180,0);
        }
        else
        {
            player.rotation = Quaternion.Euler(0,0,0);
        }
    }
    public void swap( )
    {
        
    }
    
    public void OnCollisionEnter2D(Collision2D collision)
    {
        
        if(collision.gameObject.tag == "floor")
        {
            jumping = false;
        }
    }
    
}
