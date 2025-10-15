using UnityEngine;
using UnityEngine.Video;

public class StageVideoController : MonoBehaviour
{
    [Header("�� ���������� VideoPlayer")]
    public VideoPlayer stageVideoPlayer;

    /// <summary>
    /// StageManager���� ���� VideoClip�� ���޹޾� ���
    /// </summary>
    public void PlayVideo(VideoClip clip)
    {
        stageVideoPlayer.clip = clip;
        stageVideoPlayer.isLooping = true;
        stageVideoPlayer.time = 0;
        stageVideoPlayer.Play();

        Debug.Log($"[StageVideoController] '{clip.name}' ��� ����");
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
            Debug.Log("[StageVideoController] ���� ������");
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