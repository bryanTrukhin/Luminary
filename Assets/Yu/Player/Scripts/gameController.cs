using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
// Add the namespace where PlayerController is defined, or ensure PlayerController.cs exists
// If PlayerController is in a namespace, add: using YourNamespace;

public enum PlayState { Exploring, Hiding, Puzzling, Dead, Paused }


public class GameController : MonoBehaviour
{
    public static GameController I;           // tiny singleton
    public PlayState State { get; private set; }

    [SerializeField] private JumpscareUI jumpscareUI;
    [SerializeField] private bool reloadSceneAfterDeath = false;

    public event System.Action<bool> OnHidingChanged; 

    public TMP_Text messageUI;

    void Awake() 
    {
        if(I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    // SIMPLIFIED API
    public void SetHidingState(bool isHiding)
    {
        State = isHiding ? PlayState.Hiding : PlayState.Exploring;
        
        // Notify anyone listening (like AI or Music)
        OnHidingChanged?.Invoke(isHiding);
        
        Debug.Log($"Game State Changed: {State}");
    }

    public void KillPlayer(PlayerController p)
    {
        if (State == PlayState.Dead) return;

        State = PlayState.Dead;

        p.SetMoveable(false);
        p.SetVisible(true);
        p.Lantern.SetVisible(false);

        Debug.Log("GameController: Player died, triggering jumpscare.");

        StartCoroutine(DeathSequence(p));
    }

    private IEnumerator DeathSequence(PlayerController p)
    {
        if (jumpscareUI != null)
            yield return jumpscareUI.PlayJumpscare();

        // You can fade to black or simply reload scene here
        if (reloadSceneAfterDeath)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    public bool PlayerSafeFromMonster() => State == PlayState.Hiding;

    public void writeMessage(string s)
    {
        messageUI.text = s;
    }
}