using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
        {
            GameController.I.SetRespawnPoint(transform.position);
        }
        Debug.Log($"Checkpoint triggered by: {collision.gameObject.name}");
    }
}