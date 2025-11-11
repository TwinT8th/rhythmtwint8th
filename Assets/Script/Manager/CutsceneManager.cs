using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;



public class CutsceneManager : MonoBehaviour
{
    [Header("컷씬 세트 연결")]
    public CutsceneSet currentCutsceneSet;

    [Header("Timeline")]
    public PlayableDirector director;

    [Header("이미지 레이어 관리")]
    public List<Image> cutsceneImageObjects;

    [Header("컷씬 이미지 관리")]
    public Image cutsceneImage; //화면에 표시될 이미지
    public List<Sprite> cutsceneSprites; //프레임 이미지들
    public List<CutsceneSegment> segments; // 구간 리스트

    [Header("텍스트 관리 (TextMeshPro)")]
    public List<TextMeshProUGUI> dialogueTextObjects; // 여러 TMP 오브젝트 연결

    [Header("GIF 재생 설정")]
    private Coroutine playCoroutine;
    public int frameRate = 8;        // 초당 프레임 수 (FPS)


    // Start is called before the first frame update
    void Start()
    {
        if (director !=null)
        {
            director.stopped += OnTimelineEnd;
        }

        if (currentCutsceneSet != null)
            LoadCutsceneSet(currentCutsceneSet);

        if (cutsceneSprites != null && cutsceneSprites.Count > 0 && segments.Count > 0)
            playCoroutine = StartCoroutine(PlaySegments());
    }
    private IEnumerator PlaySegments()
    {
        foreach (var segment in segments)
        {
            yield return StartCoroutine(PlaySegment(segment));
        }

    }

    private IEnumerator PlaySegment(CutsceneSegment seg)
    {
        int frameCount = seg.endFrame - seg.startFrame + 1;
        float frameInterval = 1f / frameRate;
        float elapsed = 0f;
        int frameIndex = 0;

        // 텍스트: 타겟별로 묶어서 "delay 오름차순" 순차 재생 코루틴 시작
        StopAndClearTexts();                            // 이전 세그먼트 잔여 코루틴/텍스트 정리
        float segmentStartTime = Time.time;


        // 이미지 레이어 재생
        foreach (var layer in seg.imageLayers)
        {
            StartCoroutine(PlayImageLayer(layer));
        }

        if (seg.texts.Count > 0 && dialogueTextObjects.Count > 0)
        {
            // targetIndex -> 리스트
            Dictionary<int, List<CutsceneText>> groups = new Dictionary<int, List<CutsceneText>>();
            foreach (var t in seg.texts)
            {
                if (t.targetIndex < 0 || t.targetIndex >= dialogueTextObjects.Count) continue;
                if (!groups.ContainsKey(t.targetIndex)) groups[t.targetIndex] = new List<CutsceneText>();
                groups[t.targetIndex].Add(t);
            }
            // 타겟별로 delay 오름차순 정렬 후, 각 타겟에 1개 코루틴만 띄움(겹침 방지)
            foreach (var kv in groups)
            {
                kv.Value.Sort((a, b) => a.delay.CompareTo(b.delay));
                var co = StartCoroutine(PlayTextSequenceForTarget(kv.Key, kv.Value, segmentStartTime));
                activeTextCoroutines.Add(co);
            }
        }

        // 🔸 프레임 애니 루프
        while (elapsed < seg.playDuration)
        {
            int currentFrame = seg.startFrame + frameIndex;
            if (currentFrame >= 0 && currentFrame < cutsceneSprites.Count)
                cutsceneImage.sprite = cutsceneSprites[currentFrame];

            frameIndex++;
            if (frameIndex >= frameCount)
            {
                if (seg.loop) frameIndex = 0;
                else break;
            }

            yield return new WaitForSeconds(frameInterval);
            elapsed += frameInterval;
        }

        // 세그먼트 종료 시 텍스트 코루틴 정리
        StopAndClearTexts();
    }

    // ─── 텍스트 스케줄링 보조들 ───────────────────────────────────────────────
    private readonly List<Coroutine> activeTextCoroutines = new List<Coroutine>();

    private void StopAndClearTexts()
    {
        foreach (var co in activeTextCoroutines)
            if (co != null) StopCoroutine(co);
        activeTextCoroutines.Clear();

        // 출력 중이던 텍스트 전부 정리
        foreach (var tmp in dialogueTextObjects)
            if (tmp) tmp.text = "";
    }

    private IEnumerator PlayTextSequenceForTarget(int targetIndex, List<CutsceneText> sequence, float segmentStartTime)
    {
        var tmp = dialogueTextObjects[targetIndex];

        foreach (var t in sequence)
        {
            while (Time.time - segmentStartTime < t.delay)
                yield return null;

            if (tmp)
            {
                // 🔹 위치 이동 (선택사항)
                if (t.anchoredPos != Vector2.zero)
                    tmp.rectTransform.anchoredPosition = t.anchoredPos;

                tmp.text = t.text;
            }

            float endTime = Time.time + Mathf.Max(0f, t.showDuration);
            while (Time.time < endTime)
                yield return null;

            if (tmp) tmp.text = "";
        }
    }
    private class TextState
    {
        public CutsceneText data;
        public bool shown;
        public float endTime;
        public TextState(CutsceneText d) { data = d; shown = false; endTime = 0f; }
    }


    private IEnumerator PlayImageLayer(CutsceneImageLayer layer)
    {
        if (layer.targetIndex < 0 || layer.targetIndex >= cutsceneImageObjects.Count)
            yield break;

        Image img = cutsceneImageObjects[layer.targetIndex];
        img.rectTransform.anchoredPosition = layer.anchoredPos;
        img.rectTransform.sizeDelta = layer.size;

        Color c = img.color;
        c.a = layer.alpha;
        img.color = c;

        int frameCount = layer.endFrame - layer.startFrame + 1;
        float frameInterval = layer.playDuration / frameCount;
        float elapsed = 0f;
        int frameIndex = 0;

        while (elapsed < layer.playDuration)
        {
            int currentFrame = layer.startFrame + frameIndex;
            if (currentFrame >= 0 && currentFrame < cutsceneSprites.Count)
                img.sprite = cutsceneSprites[currentFrame];

            frameIndex++;
            if (frameIndex >= frameCount)
            {
                if (layer.loop) frameIndex = 0;
                else break;
            }

            yield return new WaitForSeconds(frameInterval);
            elapsed += frameInterval;
        }
    }
    public void LoadCutsceneSet(CutsceneSet set)
    {
        cutsceneSprites = set.cutsceneSprites;
        segments = set.segments;
        Debug.Log($"컷씬 세트 로드 완료: {set.name}");
    }

    private void OnTimelineEnd(PlayableDirector dir)
    {
        Debug.Log("컷씬이 끝났습니다!");
        // 다음 씬 전환이나 UI 활성화 등 처리
    }
}
