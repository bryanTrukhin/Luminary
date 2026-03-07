using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
// Add the namespace where PlayerController is defined, or ensure PlayerController.cs exists
// If PlayerController is in a namespace, add: using YourNamespace;

public enum PlayState { Exploring, Hiding, Dead, Paused }


public class GameController : MonoBehaviour
{
    public static GameController I;           // tiny singleton
    public PlayState State { get; private set; }

    [SerializeField] private JumpscareUI jumpscareUI;
    [SerializeField] private bool reloadSceneAfterDeath = false;

    public event System.Action<bool> OnHidingChanged; 

    public TMP_Text messageUI;
    private Vector3 _currentRespawnPos;
    [SerializeField] private GameObject normalLantern;
    [SerializeField] private GameObject brokenLantern;

    private ILantern _normalLantern;
    private ILantern _brokenLantern;


    void Awake() 
    {
        if(I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        PlayerController p = FindObjectOfType<PlayerController>();
        if (p != null) _currentRespawnPos = p.transform.position;

        _normalLantern = normalLantern.GetComponent<ILantern>();
        _brokenLantern = brokenLantern.GetComponent<ILantern>();
    }

    // SIMPLIFIED API
    public void SetHidingState(bool isHiding)
    {
        State = isHiding ? PlayState.Hiding : PlayState.Exploring;
        
        // Notify anyone listening (like AI or Music)
        OnHidingChanged?.Invoke(isHiding);
        
        Debug.Log($"Game State Changed: {State}");
    }
    public void SetRespawnPoint(Vector3 newPos)
    {
        _currentRespawnPos = newPos;
        writeMessage("Checkpoint Reached");
    }

    public void KillPlayer(PlayerController p)
    {
        if (State == PlayState.Dead) return;

        State = PlayState.Dead;

        p.SetMoveable(false);
        p.SetVisible(true);
        p.Lantern.SetVisible(false);

        Debug.Log("GameController: Player died.");

        StartCoroutine(DeathSequence(p));
    }

    private IEnumerator DeathSequence(PlayerController p)
    {
        if (jumpscareUI != null)
            yield return jumpscareUI.PlayJumpscare();
        
        // if (reloadSceneAfterDeath)
        // {
        //     SceneManager.LoadScene(
        //         SceneManager.GetActiveScene().buildIndex
        //     );
        // }
        // else
        // {
            State = PlayState.Exploring; 
            p.transform.position = _currentRespawnPos;
            p.RespawnReset(); 
            Debug.Log("Player Respawned at: " + _currentRespawnPos);
            if(jumpscareUI != null)
        {
            yield return jumpscareUI.Unfade();
        }
        // }
    }

    public bool PlayerSafeFromMonster() => State == PlayState.Hiding;

    public void writeMessage(string s)
    {
        messageUI.text = s;
    }
}