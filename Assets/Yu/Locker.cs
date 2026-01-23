using UnityEngine;


public class Locker : MonoBehaviour
{
    [SerializeField] private Transform hidePoint;   // where player snaps to
    bool occupied;
    bool playerInRange;
    PlayerController occupant;
    Vector3 returnPosition; // where to put them when exiting

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Locker trigger enter with {collision.name}, tag={collision.tag}, layer={collision.gameObject.layer}");

        if (!collision.CompareTag("Player")) return;

        // cache player controller from this object or parents
        occupant = collision.GetComponentInParent<PlayerController>();
        if (occupant == null)
        {
            Debug.LogWarning($"Locker: Player collider {collision.name} had no PlayerController in parents", collision);
            return;
        }

        playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // if we’re not currently occupying the locker, clear range
        if (!occupied)
        {
            playerInRange = false;
            occupant = null;
        }
    }

    void Update()
    {
        // unified interact handling here
        if (!InputSystem.Interact()) return;

        // Try to enter
        if (!occupied && playerInRange && occupant != null)
        {
            if (hidePoint == null)
            {
                Debug.LogError("Locker: hidePoint is NOT assigned in inspector!", this);
                return;
            }
            if (GameController.I == null)
            {
                Debug.LogError("Locker: GameController.I is null — is there a GameController in the scene?", this);
                return;
            }

            // remember where we were standing
            returnPosition = occupant.transform.position;

            // move into locker
            occupant.transform.position = hidePoint.position;
            GameController.I.EnterLocker(occupant);
            occupied = true;
            return;
        }

        // Try to exit
        if (occupied && occupant != null)
        {
            if (GameController.I == null)
            {
                Debug.LogError("Locker: GameController.I is null on exit!", this);
                occupied = false;
                return;
            }

            // pop back out to where we entered
            occupant.transform.position = returnPosition;

            GameController.I.ExitLocker(occupant);
            occupied = false;
            // don't null occupant here so we can re-enter without leaving trigger;
            // it will be cleared on trigger exit if needed
        }
    }
}
