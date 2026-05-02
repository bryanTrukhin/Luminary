using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailCollider : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Player")) != 0)
        {
            Debug.Log("TAIL HIT PLAYER!");
            //other.GetComponent<Player>().TakeDamage(damage);
        }
    }
}
