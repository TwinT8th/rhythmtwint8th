using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    [Header("캔버스 참조")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;    // 검은색 페이드용
    [SerializeField] private CanvasGroup loadingCanvasGroup; // 실제 로딩창용

    [Header("설정값")]
    [SerializeField] private float fadeDuration = 0.4f;   // 페이드 인/아웃 시간
    [SerializeField] private float loadingDuration = 0.8f; // 로딩창 표시 시간

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;
        if (loadingCanvasGroup != null)
            loadingCanvasGroup.alpha = 0f;
    }

    // ?? 빠른 페이드 (0.4초간 어두워졌다가 다시 밝아짐)
    public void FadeQuick()
    {
        if (fadeCanvasGroup == null) return;
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        fadeCanvasGroup.gameObject.SetActive(true);

        // 페이드 아웃 (화면 어둡게)
        yield return StartCoroutine(FadeCanvas(fadeCanvasGroup, 0f, 1f, fadeDuration * 0.5f));

        // 짧은 딜레이 후
        yield return new WaitForSeconds(0.1f);

        // 페이드 인 (화면 밝게)
        yield return StartCoroutine(FadeCanvas(fadeCanvasGroup, 1f, 0f, fadeDuration * 0.5f));

        fadeCanvasGroup.gameObject.SetActive(false);
    }

    // ?? 진짜 로딩창 (0.8초 표시)
    public void ShowLoading()
    {
        if (loadingCanvasGroup == null) return;
        StartCoroutine(ShowLoadingRoutine());
    }

    private IEnumerator ShowLoadingRoutine()
    {
        loadingCanvasGroup.gameObject.SetActive(true);

        // 점점 나타남
        yield return StartCoroutine(FadeCanvas(loadingCanvasGroup, 0f, 1f, 0.2f));

        // 잠시 유지
        yield return new WaitForSeconds(loadingDuration);

        // 점점 사라짐
        yield return StartCoroutine(FadeCanvas(loadingCanvasGroup, 1f, 0f, 0.2f));

        loadingCanvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}