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
            Destroy(instance.gameObject);  // ���� �Ŵ��� ����

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

    // ���� PlayVideo() �״�� ����
    public void PlayVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
            videoPlayer.Play();
    }

    // ���� �߰��� "PlayVideo(string videoName)" - Ư�� ���� �����
    public void PlayVideo(string videoName)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[VideoManager] VideoPlayer�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        VideoClip clip = Resources.Load<VideoClip>($"Video/{videoName}");
        if (clip == null)
        {
            Debug.LogError($"[VideoManager] '{videoName}.mp4'��(��) Resources/Video �������� ã�� �� �����ϴ�.");
            return;
        }

        videoPlayer.clip = clip;
        videoPlayer.isLooping = true;
        videoPlayer.time = 0;
        videoPlayer.Play();

        Debug.Log($"[VideoManager] ���� '{videoName}.mp4' ��� ����");
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
