using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatKing : RatBase
{
    public GameObject minionPrefab;
    public int maxMinions = 6;

    private List<GameObject> minions = new List<GameObject>();

    protected override void TryAttack()
    {
        if (player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < detectionDistance)
        {
            StartCoroutine(RollAndSpawn());
        }
    }

    IEnumerator RollAndSpawn()
    {
        isAttacking = true;

        Vector2 dir = player.position.x > transform.position.x ? Vector2.right : Vector2.left;

        rb.velocity = dir * 6f;
        yield return new WaitForSeconds(1f);

        rb.velocity = Vector2.zero;

        SpawnMinions();

        yield return new WaitForSeconds(2f);

        isAttacking = false;
    }

    void SpawnMinions()
    {
        minions.RemoveAll(m => m == null);

        if (minions.Count >= maxMinions)
        {
            Debug.Log("MAX MINIONS REACHED");
            return;
        }

        GameObject m = Instantiate(minionPrefab, transform.position + Vector3.right, Quaternion.identity);
        minions.Add(m);
    }
}
