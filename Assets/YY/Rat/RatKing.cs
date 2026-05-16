using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatKing : RatBase
{
    public GameObject[] minionPrefabs;
    public int maxMinions = 6;

    private List<GameObject> minions = new List<GameObject>();

    private bool attackCooldownLock = false;

    protected override void TryAttack()
    {
        if (player == null || isAttacking || attackCooldownLock) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < detectionDistance && !isAttacking)
        {
            StartCoroutine(RollAndSpawn());
        }

    }

    IEnumerator RollAndSpawn()
    {
        isAttacking = true;

        FacePlayer();

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

        if (minionPrefabs == null || minionPrefabs.Length == 0)
        {
            Debug.LogWarning("No minion prefabs assigned!");
            return;
        }

        int index = Random.Range(0, minionPrefabs.Length);

        GameObject prefab = minionPrefabs[index];

        Vector3 spawnPos = transform.position + new Vector3(facingDir * 1.5f, 0, 0);

        GameObject m = Instantiate(prefab, spawnPos, Quaternion.identity);

        minions.Add(m);
    }

}
