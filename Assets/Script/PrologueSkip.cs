using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrologueSkip : MonoBehaviour
{
    [Header("스킵 버튼")]
    [SerializeField] private Button skipButton;

    [Header("페이드용 이미지 (검은색 풀스크린 UI)")]
    [SerializeField] private Image fadeImage;


    [Header("페이드 설정")]
    [SerializeField] private float fadeDuration = 1f;

    private bool isSkipping = false;

    void Start()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipPressed);

        if (fadeImage != null)
            fadeImage.gameObject.SetActive(false);
    }

    public void OnSkipPressed()
    {
        if (isSkipping) return;
        isSkipping = true;
        StartCoroutine(FadeOutAndReturn());
    }

    private IEnumerator FadeOutAndReturn()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeImage.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }

            c.a = 1f;
            fadeImage.color = c;
        }

        // 모든 코루틴 및 컷씬 정지
        CutsceneManager manager = FindObjectOfType<CutsceneManager>();
        if (manager != null)
            manager.StopAllCoroutines();


        //  메인 씬 로드 완료 시 실행할 콜백 등록
        LoadingManager.instance.onSceneLoaded = () =>
        {
            var title = FindObjectOfType<TitleMenu>();
            if (title != null) title.AfterPrologue();
            else Debug.LogWarning("[PrologueSkip] TitleMenu를 찾지 못했습니다.");
        };

        // 페이드아웃이 끝나면 타이틀 씬 복귀
        LoadingManager.instance.LoadScene("Main");

    }


}