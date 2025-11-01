using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    [Header("ĵ���� ����")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;    // ������ ���̵��
    [SerializeField] private CanvasGroup loadingCanvasGroup; // ���� �ε�â��

    [Header("������")]
    [SerializeField] private float fadeDuration = 0.4f;   // ���̵� ��/�ƿ� �ð�
    [SerializeField] private float loadingDuration = 0.8f; // �ε�â ǥ�� �ð�

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;
        if (loadingCanvasGroup != null)
            loadingCanvasGroup.alpha = 0f;
    }

    // ?? ���� ���̵� (0.4�ʰ� ��ο����ٰ� �ٽ� �����)
    public void FadeQuick()
    {
        if (fadeCanvasGroup == null) return;
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        fadeCanvasGroup.gameObject.SetActive(true);

        // ���̵� �ƿ� (ȭ�� ��Ӱ�)
        yield return StartCoroutine(FadeCanvas(fadeCanvasGroup, 0f, 1f, fadeDuration * 0.5f));

        // ª�� ������ ��
        yield return new WaitForSeconds(0.1f);

        // ���̵� �� (ȭ�� ���)
        yield return StartCoroutine(FadeCanvas(fadeCanvasGroup, 1f, 0f, fadeDuration * 0.5f));

        fadeCanvasGroup.gameObject.SetActive(false);
    }

    // ?? ��¥ �ε�â (0.8�� ǥ��)
    public void ShowLoading()
    {
        if (loadingCanvasGroup == null) return;
        StartCoroutine(ShowLoadingRoutine());
    }

    private IEnumerator ShowLoadingRoutine()
    {
        loadingCanvasGroup.gameObject.SetActive(true);

        // ���� ��Ÿ��
        yield return StartCoroutine(FadeCanvas(loadingCanvasGroup, 0f, 1f, 0.2f));

        // ��� ����
        yield return new WaitForSeconds(loadingDuration);

        // ���� �����
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