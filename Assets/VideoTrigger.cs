using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine;

public class VideoTrigger : MonoBehaviour
{
    [SerializeField] public VideoPlayer endingVideo;
    [SerializeField] public AudioSource ambianceAudio;

    // NEW: A slot to hold your UI (like your Canvas)
    [SerializeField] public GameObject gameUI;

    private bool videoHasPlayed = false;

    private void Start()
    {
        // Listen for the video to finish
        endingVideo.loopPointReached += OnVideoEnd;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the zone
        if (other.CompareTag("Player") && !videoHasPlayed)
        {
            videoHasPlayed = true;

            // Stop the ambiance if it's assigned
            if (ambianceAudio != null)
            {
                ambianceAudio.Stop();
            }

            // NEW: Turn off the UI if it's assigned
            if (gameUI != null)
            {
                gameUI.SetActive(false);
            }

            endingVideo.Play();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("The video finished playing!");

        // Optional: If you aren't loading a new scene right away, 
        // you can uncomment the line below to turn the UI back on.
        // if (gameUI != null) gameUI.SetActive(true);

        // Logic for credits or main menu goes here
    }
}