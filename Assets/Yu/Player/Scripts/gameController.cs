using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
// Add the namespace where PlayerController is defined, or ensure PlayerController.cs exists
// If PlayerController is in a namespace, add: using YourNamespace;

public enum PlayState { Exploring, Hiding, Dead, Paused }


public class GameController : MonoBehaviour
{
    public static GameController I;
    public PlayState State { get; private set; }

    [SerializeField] private JumpscareUI jumpscareUI;
    [SerializeField] private bool reloadSceneAfterDeath = false;

    public event System.Action<bool> OnHidingChanged; 
    public event System.Action<bool> OnLanternStateChanged;

    public TMP_Text messageUI;
    private Vector3 _currentRespawnPos;
    private bool _islanternBroken = false;

    void Awake() 
    {
        if(I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        PlayerController p = FindObjectOfType<PlayerController>();
        if (p != null) _currentRespawnPos = p.transform.position;
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    _islanternBroken = !_islanternBroken;
        //    TriggerLanternBreak(_islanternBroken);
        //    string status = _islanternBroken ? "Broken" : "Normal";
        //    Debug.Log($"[GameController] Lantern State: {status}");
        //    writeMessage($"Lantern State: {status}");
        //}
        if (Input.GetKeyDown(KeyCode.O))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // SIMPLIFIED API
    public void SetHidingState(bool isHiding)
    {
        State = isHiding ? PlayState.Hiding : PlayState.Exploring;
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
        if(messageUI != null)
        {
            messageUI.text = s;
        }
    }
    public void TriggerLanternBreak(bool isBroken)
    {
        OnLanternStateChanged?.Invoke(isBroken);
    }

}