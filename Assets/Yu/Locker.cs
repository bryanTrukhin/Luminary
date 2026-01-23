using UnityEngine;

public class Locker : MonoBehaviour
{
    [SerializeField] private Transform hidePoint;   // Where they snap to inside
    [SerializeField] private Transform exitPoint;   // Where they appear when leaving (Optional, defaults to locker pos)

    private IHidable _occupant;     // Who is inside? (Could be Player, could be NPC)
    private IHidable _targetInRange; // Who is standing in front of the door?
    private bool _isOccupied;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object is "Hidable"
        if (collision.TryGetComponent<IHidable>(out var hidable))
        {
            _targetInRange = hidable;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IHidable>(out var hidable) && hidable == _targetInRange)
        {
            _targetInRange = null;
        }
    }

    void Update()
    {
        // 1. Check Input
        if (InputSystem.Interact())
        {
            if (_isOccupied)
            {
                ExitLocker();
            }
            else if (_targetInRange != null)
            {
                EnterLocker();
            }
        }
    }

    void EnterLocker()
    {
        if (_targetInRange == null) return;

        _occupant = _targetInRange;
        
        // 1. Tell the Object to Hide (Visuals/Physics handled by the object)
        _occupant.EnterHiding(hidePoint.position);

        // 2. Update Global Game State (So enemies know to stop chasing)
        if (GameController.I != null) 
            GameController.I.SetHidingState(true);

        _isOccupied = true;
    }

    void ExitLocker()
    {
        if (_occupant == null) return;

        // Determine exit position (use transform.position if no exitPoint set)
        Vector3 spawnPos = exitPoint != null ? exitPoint.position : transform.position;

        // 1. Tell Object to Unhide
        _occupant.ExitHiding(spawnPos);

        // 2. Update Global Game State
        if (GameController.I != null) 
            GameController.I.SetHidingState(false);

        _occupant = null;
        _isOccupied = false;
    }
}