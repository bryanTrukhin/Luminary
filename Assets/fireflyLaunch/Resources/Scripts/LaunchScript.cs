using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class LaunchScript : MonoBehaviour
{
    public GameObject fireflyPrefab;
    public GameObject flashPrefab;
    public Transform lantern;
    public float flyCooldown;
    public Vector2 flySpawn;
    [SerializeField] public bool flyUsable;
    public float flashCooldown;
    [SerializeField] public bool flashUsable;
    // Start is called before the first frame update
    void Start()
    {
        lantern = GameObject.FindGameObjectWithTag("lantern").GetComponent<Transform>();
        flySpawn = new Vector2 (lantern.position.x+1,lantern.position.y);
        flyCooldown = 5f;
        flashCooldown = 5f;
        flyUsable = true;
        flashUsable = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        flyUpdate();
        flashUpdate();
    }
    public void flyUpdate()
    {
        if (flyCooldown <=0)
        {
            flyCooldown = 5f;
            flyUsable = true;
        }
        if (!flyUsable)
        {
            flyCooldown -= Time.deltaTime;
        }
        if(Input.GetKeyDown(KeyCode.E) && flyUsable)
        {
            Instantiate(fireflyPrefab,flySpawn,lantern.rotation);
            flyUsable = false;
        }
    }
    public void flashUpdate()
    {
        if (flashCooldown <=0)
        {
            flashCooldown = 5f;
            flashUsable = true;
        }
        if (!flashUsable)
        {
            flashCooldown -= Time.deltaTime;
        }
        if(Input.GetKeyDown(KeyCode.C) && flashUsable)
        {
            Instantiate(flashPrefab,lantern.position,lantern.rotation);
            flashUsable = false;
        }
    }
}
