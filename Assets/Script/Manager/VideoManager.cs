using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public static VideoManager instance;

    [Header("Video Player")]
    public VideoPlayer videoPlayer;

    [SerializeField] VideoPlayer earth = null;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(instance.gameObject);  // 기존 매니저 제거

        instance = this;

    }


    // Start is called before the first frame update
    void Start()
    {


        videoPlayer.isLooping = true;
        //videoPlayer.Play();
    }

    public void PauseVideo()
    {
        if(videoPlayer.isPlaying)
            videoPlayer.Pause();
    }

    // 기존 PlayVideo() 그대로 유지
    public void PlayVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
            videoPlayer.Play();
    }

    // 새로 추가된 "PlayVideo(string videoName)" - 특정 영상 재생용
    public void PlayVideo(string videoName)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[VideoManager] VideoPlayer가 할당되지 않았습니다!");
            return;
        }

        VideoClip clip = Resources.Load<VideoClip>($"Video/{videoName}");
        if (clip == null)
        {
            Debug.LogError($"[VideoManager] '{videoName}.mp4'을(를) Resources/Video 폴더에서 찾을 수 없습니다.");
            return;
        }

        videoPlayer.clip = clip;
        videoPlayer.isLooping = true;
        videoPlayer.time = 0;
        videoPlayer.Play();

        Debug.Log($"[VideoManager] 비디오 '{videoName}.mp4' 재생 시작");
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

    public void RestartEarth()
    {
        earth.time = 0;
        earth.Play();
    }

    public void PauseEarth()
    {
        if (earth.isPlaying)
            earth.Pause();
    }



}
