using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;


public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;
    public Action onSceneLoaded;

    [Header("페이드용 이미지 (검은색 풀스크린 UI)")]
    [SerializeField] private Image fadeImage;

    [Header("페이드 설정")]
    [SerializeField] private float fadeDuration = 1f;

    private bool isFading = false;

    private void Awake()
    {
        // 싱글톤: 씬 전환 시 파괴되지 않음
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    /// <summary>
    /// 씬 전환 (기본: 페이드아웃 → 씬 로드 → 페이드인)
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!isFading)
            StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        // 씬이 완전히 로드된 시점에 콜백 실행
        onSceneLoaded?.Invoke();
        onSceneLoaded = null;              // 재호출 방지 (중요)

        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(Fade(1f, 0f));

        isFading = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeImage == null)
            yield break;

        fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;

        // 완전히 투명해지면 비활성화
        if (endAlpha == 0f)
            fadeImage.gameObject.SetActive(false);
    }
}
