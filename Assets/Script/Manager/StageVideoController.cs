using UnityEngine;
using UnityEngine.Video;

public class StageVideoController : MonoBehaviour
{
    [Header("이 스테이지의 VideoPlayer")]
    public VideoPlayer stageVideoPlayer;

    /// <summary>
    /// StageManager에서 직접 VideoClip을 전달받아 재생
    /// </summary>
    public void PlayVideo(VideoClip clip)
    {
        stageVideoPlayer.clip = clip;
        stageVideoPlayer.isLooping = true;
        stageVideoPlayer.time = 0;
        stageVideoPlayer.Play();

        Debug.Log($"[StageVideoController] '{clip.name}' 재생 시작");
    }
    public void PauseVideo()
    {
        if (stageVideoPlayer != null) stageVideoPlayer.Pause();
    }
    public void StopVideo()
    {
        if (stageVideoPlayer != null && stageVideoPlayer.isPlaying)
        {
            stageVideoPlayer.Stop();
            Debug.Log("[StageVideoController] 영상 정지됨");
        }
    }

    public void PlayVideo()
    {
        stageVideoPlayer.Play();
    }


    public void RestartVideo()
    {
        stageVideoPlayer.time = 0;
        stageVideoPlayer.Play();
    }

}