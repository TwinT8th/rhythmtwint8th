using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimatorBPM : MonoBehaviour
{
    [Header("Sprite Frames")]
    public Sprite[] frames;
    public Image targetImage;
    public SpriteRenderer targetRenderer;

    [Header("BPM Settings")]
    public float bpm = 90f;
    public float beatsToPlay = 3f;
    public bool autoPlay = false;

    private int currentFrame = 0;
    private float timer = 0f;
    private float frameDuration = 0f;
    private bool isPlaying = false;



    void Update()
    {
        if (!isPlaying || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame++;

            if (currentFrame < frames.Length)
            {
                ApplyFrame();
            }
            else
            {
                //Debug.Log("[SpriteAnimatorBPM] Animation finished");
                currentFrame = frames.Length - 1;
                ApplyFrame();
                isPlaying = false;
            }
        }
    }

    public void Play()
    {
        if (frames.Length == 0) return;

        float totalDuration = (60f / bpm) * beatsToPlay;
        frameDuration = totalDuration / frames.Length;

        currentFrame = 0;
        timer = 0f;
        isPlaying = true;

        ApplyFrame();

        if (targetImage != null) targetImage.enabled = true;
        if (targetRenderer != null) targetRenderer.enabled = true;
        //Debug.Log($"[{name}] Play() 실행됨, frames={frames.Length}, frameDuration={frameDuration}");
    }

    /// <summary>
    /// 즉시 멈추기 (기존 Stop)  이미지를 바로 숨김
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        timer = 0f;

        if (targetImage != null) targetImage.enabled = false;
        if (targetRenderer != null) targetRenderer.enabled = false;
    }
    /// <summary>
    /// 마지막 프레임에서 정지 (이미지는 그대로 유지)
    /// </summary>
    public void StopOnLastFrame()
    {
        if (frames.Length == 0) return;

        isPlaying = false;
        currentFrame = frames.Length - 1;
        timer = 0f;

        ApplyFrame(); // 마지막 프레임 적용
        if (targetImage != null) targetImage.enabled = true;
        if (targetRenderer != null) targetRenderer.enabled = true;
    }
    private void ApplyFrame()
    {
        if (currentFrame < 0 || currentFrame >= frames.Length) return;

        if (targetImage != null)
            targetImage.sprite = frames[currentFrame];
        if (targetRenderer != null)
            targetRenderer.sprite = frames[currentFrame];
    }
}