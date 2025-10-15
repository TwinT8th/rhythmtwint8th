using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


[System.Serializable]
public class StageData
{
    public string stageName;
    public string bgmName;
    public string patternFile;
    public GameObject[] characterPrefabs;  // 곡당 여러 캐릭터 가능 (희소한 + 고양이)
    public float bpm = 90f;
    public VideoClip stageVideo;            //Inspector에서 직접 등록할 영상
}

public class StageManager : MonoBehaviour
{

    public static StageManager instance;

    [Header("곡 리스트 (각 곡의 메타 정보)")]
    public StageData[] songList;

    [Header("비디오 플레이어 (Inspector에 등록)")]
    public VideoPlayer stageVideoPlayer;

    [Header("현재 활성화된 오브젝트")]
    public GameObject currentCharacter;


    // 캐릭터 오브젝트 풀
    private readonly Dictionary<string, GameObject> characterPool = new Dictionary<string, GameObject>();

    // 현재 활성화된 캐릭터들
    private readonly List<GameObject> activeCharacters = new List<GameObject>();

    //애니메이션 기본 시작 상태
    private const string DEFAULT_STATE = "Walk";

    StageVideoController stageVideo;


    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        stageVideo = FindObjectOfType<StageVideoController>();
    }
    /// <summary>
    /// 선택된 곡(songIndex)에 맞춰 배경 영상, 캐릭터, BPM 등을 교체한다.
    /// </summary>

    public void LoadSong(int songIndex)
    {
        Debug.Log($"[StageManager] LoadSong({songIndex}) 호출됨");

        if (songIndex < 0 || songIndex >= songList.Length)
        {
            Debug.LogError($"[StageManager] 잘못된 songIndex: {songIndex}");
            return;
        }

        StageData data = songList[songIndex];


        // 비디오 교체 (Inspector에서 등록한 VideoPlayer 사용, stageVideoController에 영상 전달)
        if (stageVideo != null && data.stageVideo != null)
        {

            stageVideo.PlayVideo(data.stageVideo);
            stageVideo.stageVideoPlayer.time = 0; // 수동으로 0초로 이동
            stageVideo.stageVideoPlayer.Play();   // 재생 재개
            Debug.Log($"[StageManager] '{data.stageVideo.name}' 영상 재생 시작");
        }
        else
        {
            Debug.LogWarning("[StageManager] stageVideo 또는 stageVideoPlayer가 없습니다.");
        }


        // 기존 캐릭터 비활성화
        foreach (var obj in activeCharacters)
        {
            if (obj != null) obj.SetActive(false);
        }
        activeCharacters.Clear();

        // 이번 곡의 캐릭터들 활성화
        for (int i = 0; i < data.characterPrefabs.Length; i++)
        {
            var prefab = data.characterPrefabs[i];
            if (prefab == null) continue;

            // 곡 인덱스 + 프리팹 이름으로 키 생성 (곡마다 같은 이름 프리팹이 있어도 안전)
            string key = $"{songIndex}_{prefab.name}";
            GameObject character;

            if (characterPool.TryGetValue(key, out character))
            {
                // 풀 재사용
                character.SetActive(true);
                Debug.Log($"[StageManager] 캐릭터 재사용: {key}");
            }
            else
            {
                // 새로 생성 + 풀 등록
                character = Instantiate(prefab);
                character.name = key; // 보기 좋게 이름 맞춰둠
                characterPool[key] = character;
                Debug.Log($"[StageManager] 캐릭터 생성 및 풀 등록: {key}");
            }

            // ⭐️ Animator/컨트롤러/속도/기본 상태 초기화 (재사용/신규 모두 동일 루틴)
            InitCharacter(character, data.bpm);

            activeCharacters.Add(character);

            // 메인 캐릭터 등록(첫 번째 것만)
            if (i == 0)
            {
                currentCharacter = character;
                var controller = character.GetComponentInChildren<CharactorController>(true);
                if (TimingManager.instance != null && controller != null)
                {
                    TimingManager.instance.charactor = controller;
                    Debug.Log($"[StageManager] TimingManager에 '{controller.name}' 캐릭터 등록 완료");
                }
                else if (controller == null)
                {
                    Debug.LogError($"[StageManager] {character.name}에 CharactorController가 없습니다!");
                }
            }
        

        // BPM 전달
        if (NoteManager.instance != null)
                NoteManager.instance.bpm = data.bpm;
        }
    }

    /// <summary>
    /// 풀 재사용/신규 생성 상관없이 캐릭터를 동일한 방식으로 초기화
    /// </summary>
    private void InitCharacter(GameObject character, float bpm)
    {
        // Animator는 루트가 아니라 자식에 붙어 있을 수도 있으므로 InChildren으로 탐색
        Animator anim = character.GetComponentInChildren<Animator>(true);
        if (anim == null)
        {
            Debug.LogError($"[StageManager] Animator를 찾을 수 없습니다. (obj={character.name})");
            return;
        }

        // Animator 기본 설정
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        anim.updateMode = AnimatorUpdateMode.Normal; // AnimatePhysics면 안 됨
        anim.enabled = true;

        // CharactorController가 있으면 BPM 즉시 반영
        var controller = character.GetComponentInChildren<CharactorController>(true);
        if (controller != null)
            controller.UpdateAnimatorSpeed(bpm);

        // 한 프레임 기다렸다가 Animator 완전 초기화 (이게 핵심)
        StartCoroutine(InitAnimatorNextFrame(anim, character.name));
    }

    private IEnumerator InitAnimatorNextFrame(Animator anim, string charName)
    {
        // ✅ 한 프레임 대기 — Animator가 Scene에 등록되고 Controller 연결 완료될 때까지
        yield return null;

        // 완전 리셋
        anim.Rebind();
        anim.Update(0f);

        // 기본 상태명 (Walk 또는 Idle, 실제 사용 중인 상태 이름으로 바꿔도 됨)
        string defaultState = "Walk";

        if (anim.HasState(0, Animator.StringToHash(defaultState)))
        {
            anim.Play(defaultState, 0, 0f);
            anim.Update(0f);
            Debug.Log($"[StageManager] '{charName}' → '{defaultState}' 상태에서 애니메이션 시작됨");
        }
        else
        {
            Debug.LogWarning($"[StageManager] '{charName}'에 기본 상태 '{defaultState}'을(를) 찾을 수 없습니다. Animator 상태 이름 확인 필요!");
        }

        // 현재 재생 중인 클립 정보 디버그
        var clips = anim.GetCurrentAnimatorClipInfo(0);
        if (clips != null && clips.Length > 0)
            Debug.Log($"[Animator Test] {charName} 현재 재생 중 클립: {clips[0].clip.name}");
        else
            Debug.LogWarning($"[Animator Test] {charName} → 재생 중 클립이 없습니다!");
    }

    private static bool HasState(Animator anim, int layer, string stateName)
    {
        return anim.HasState(layer, Animator.StringToHash(stateName));
    }

    /// <summary>
    /// 스테이지 종료 시, 캐릭터 비활성화
    /// </summary>
    public void DeactivateAllCharacters()
    {
        foreach (var obj in activeCharacters)
        {
            if (obj != null) obj.SetActive(false);
        }
        activeCharacters.Clear();
    }


}


