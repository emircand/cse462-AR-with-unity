using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private bool firstPlay = true; // Flag to check if the video has been played for the first time

    void Start()
    {
        // Get the VideoPlayer component attached to the GameObject
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void Update()
    {
        // Check for a mouse click or touch input
        if (Input.GetMouseButtonDown(0)) // 0 represents the left mouse button or a single touch
        {
            // Check if the ray hits this object
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                if (firstPlay)
                {
                    // Play the video for the first time
                    videoPlayer.Play();
                    firstPlay = false;
                }
                else
                {
                    // Toggle play/pause
                    if (videoPlayer.isPlaying)
                    {
                        videoPlayer.Pause();
                    }
                    else
                    {
                        videoPlayer.Play();
                    }
                }
            }
        }
    }
}