using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class JumpscareUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image jumpscareImage;    // big scary image
    [SerializeField] private Image fadeImage;         // optional fade to black
    [SerializeField] private AudioSource screech;     // optional sound

    [Header("Timing")]
    [SerializeField] private float scareHoldTime = 0.75f;
    [SerializeField] private float fadeTime = 0.1f;

    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;

        if (jumpscareImage != null)
            jumpscareImage.enabled = false;
    }

    /// <summary>Shows jumpscare, plays audio, fades, returns via coroutine.</summary>
    public IEnumerator PlayJumpscare()
    {
        // Show scare image
        if (jumpscareImage != null)
            jumpscareImage.enabled = true;

        if (screech != null)
            screech.Play();

        // fade canvas in instantly for a punch
        _canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(scareHoldTime);

        // Fade to black afterward
        if (fadeImage != null)
            yield return FadeToBlack();
    }

    IEnumerator FadeToBlack()
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeTime);
            fadeImage.color = c;
            yield return null;
        }
    }

    public IEnumerator Unfade()
    {
        if (jumpscareImage != null)
            jumpscareImage.enabled = false;
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeTime);
            fadeImage.color = c;
            yield return null;
        }
        _canvasGroup.alpha = 0f;
    }
}