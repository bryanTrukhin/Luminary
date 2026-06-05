using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine;

public class VideoTrigger : MonoBehaviour
{
    [SerializeField] public VideoPlayer endingVideo;
    [SerializeField] public AudioSource ambianceAudio;
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

            // NEW: Stop the ambiance if it's assigned
            if (ambianceAudio != null)
            {
                ambianceAudio.Stop();
            }

            endingVideo.Play();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("The video finished playing!");
        // Logic for credits or main menu goes here
    }
}