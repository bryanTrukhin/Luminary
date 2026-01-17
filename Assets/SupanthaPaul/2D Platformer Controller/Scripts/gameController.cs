using System.Collections;
using UnityEngine;

public enum PlayState { Exploring, Hiding, Puzzling, Dead, Paused }


namespace SupanthaPaul
{
    public class GameController : MonoBehaviour
    {
        public static GameController I;           // tiny singleton
        public PlayState State { get; private set; }

        public event System.Action OnLockerEntered;
        public event System.Action OnLockerExited;

        [SerializeField] private JumpscareUI jumpscareUI;
        [SerializeField] private bool reloadSceneAfterDeath = false;



        void Awake() 
        {
            if(I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }
            I = this;
        }

        /* --------- PUBLIC API ---------- */
        public void EnterLocker(PlayerController p)
        {
            OnLockerEntered?.Invoke();
            State = PlayState.Hiding;

            p.transform.SetParent(transform, true);
            p.SetPlayable(false);     
            p.SetVisible(false);          // <- hide all sprites
            p.Lantern.SetOn(false);       // turn off light
        }

        public void ExitLocker(PlayerController p)
        {
            OnLockerExited?.Invoke();
            State = PlayState.Exploring;

            p.transform.SetParent(null, true);
            p.SetPlayable(true);
            p.SetVisible(true);           // <- show sprites again
            p.Lantern.SetOn(true);        // turn light back on
        }

        public void KillPlayer(PlayerController p)
        {
            if (State == PlayState.Dead) return;

            State = PlayState.Dead;

            p.SetPlayable(false);
            p.SetVisible(true);
            p.Lantern.SetOn(false);

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
    }

}