using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public static VideoManager instance;

    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(instance.gameObject);  // 기존 매니저 제거

        instance = this;

    }


    // Start is called before the first frame update
    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    public void PauseVideo()
    {
        if(videoPlayer.isPlaying)
            videoPlayer.Pause();
    }

    public void PlayVideo()
    {
        if (!videoPlayer.isPlaying)
            videoPlayer.Play();
    }
    public void RestartVideo()
    {
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    public double GetVideoTime()
    {
        return videoPlayer.time;
    }
}
