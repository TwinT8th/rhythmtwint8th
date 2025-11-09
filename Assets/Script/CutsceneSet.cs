
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cutscene/CutsceneSet")]
public class CutsceneSet : ScriptableObject
{
    [Header("컷씬 스프라이트들 (순서대로 프레임)")]
    public List<Sprite> cutsceneSprites = new List<Sprite>();

    [Header("컷씬 세그먼트 리스트")]
    public List<CutsceneSegment> segments = new List<CutsceneSegment>();
}

/*
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cutscene/CutsceneSet")]
public class CutsceneSet : ScriptableObject
{
    [Header("한 세트에서 공용으로 쓸 스프라이트 모음(아틀라스 느낌)")]
    public List<Sprite> atlas = new List<Sprite>();

    [Header("동시에/순차로 재생될 레이어들")]
    public List<CutLayer> layers = new List<CutLayer>();
}

[System.Serializable]
public class CutLayer
{
    [Header("식별/정렬용")]
    public string name = "Layer";
    public int sortingOrder = 0;              // 큰 값이 위로 (Hierarchy용 정렬)

    [Header("프레임 범위(포함, 예: 8~11)")]
    public int startFrame = 0;                 // atlas에서 시작 인덱스(포함)
    public int endFrame = 0;                   // atlas에서 끝 인덱스(포함)

    [Header("재생 파라미터")]
    public float fps = 15f;                    // 초당 프레임
    public float startTime = 0f;               // 컷씬 시작 후 몇 초에 시작할지
    public float duration = 2f;                // 재생 총 시간(초)
    public bool loop = true;                   // 재생시간 동안 프레임 루프할지
    public bool hideWhenFinished = true;       // duration 지난 뒤 감출지(비루프 혹은 종료 후)

    [Header("간단 연출 옵션(선택)")]
    public bool blink = false;                 // 깜빡임
    public float blinkHz = 8f;                 // 깜빡임 빈도
    public bool shake = false;                 // 흔들림
    public float shakeAmount = 2f;             // 흔들림 세기(픽셀 단위 감각)

    [Header("UI 배치(캔버스 기준)")]
    public Vector2 anchoredPos = Vector2.zero; // 좌표
    public Vector2 size = new Vector2(256, 256);
    public Vector2 pivot = new Vector2(0.5f, 0.5f);
    public float alpha = 1f;
}
*/