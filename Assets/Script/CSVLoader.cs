// 파일명: CSVLoader.cs  (반드시 이 이름과 클래스명이 일치해야 함)

using System;
using System.Collections.Generic;
using System.Globalization;   // 소수점 파싱을 '.'로 고정하기 위함
using System.IO;              // StringReader 사용
using UnityEngine;

/// <summary>
/// CSV 한 줄 = 특정 박자(beat)에 특정 좌표(x, y)로 노트가 스폰됨을 의미
/// </summary>
[Serializable]
public struct NoteEvent
{
    public float beat;    // 몇 박자 시점인지 (0.0부터 시작 가능)
    public float x;       // 스폰 X 좌표 (월드/로컬 어떤 좌표계인지는 스포너에서 정의)
    public float y;       // 스폰 Y 좌표

    public NoteEvent(float beat, float x, float y)
    {
        this.beat = beat;
        this.x = x;
        this.y = y;
    }

    public Vector2 Position => new Vector2(x, y); // 편의용
}

/// <summary>
/// Resources의 CSV(TextAsset)를 읽어 List<NoteEvent>로 변환하는 로더.
/// - CSV 헤더: beat, x, y (대소문자 무시)
/// - 빈 줄, # 주석 라인 무시
/// - 구분자: 콤마/세미콜론/탭 자동 감지
/// - 소수점: 항상 '.' 으로 표기(, 사용 금지)
/// </summary>
public class CSVLoader : MonoBehaviour
{
    [Header("입력 소스(둘 중 하나 사용)")]
    [Tooltip("Resources 기준 경로(확장자 제외). 예: \"pattern0\" 또는 \"Patterns/pattern0\"")]
    public string resourcePath = "pattern0";

    [Tooltip("직접 TextAsset을 드래그해 넣어서 사용 가능 (리소스 경로보다 우선)")]
    public TextAsset csvAsset;

    [Header("출력 데이터 (읽기 용도로 사용)")]
    [SerializeField] private List<NoteEvent> pattern = new List<NoteEvent>();

    /// <summary>
    /// 다른 스크립트에서 읽을 때 사용. (참조를 그대로 넘기므로 수정 주의)
    /// </summary>
    public List<NoteEvent> GetPattern() => pattern;

    private void Awake()
    {
        // 실행 시 자동 로드. 실패하면 콘솔에 에러 출력.
        if (!TryLoad())
        {
            Debug.LogError("[CSVLoader] CSV 로드에 실패했습니다. 경로나 컴파일 에러를 확인하세요.");
        }
    }

    /// <summary>
    /// 설정된 csvAsset 또는 resourcePath를 사용해 CSV를 읽어 pattern을 갱신.
    /// </summary>
    public bool TryLoad()
    {
        // 1) 입력 소스 결정: 인스펙터에 TextAsset이 있으면 그걸 우선 사용
        TextAsset src = csvAsset;
        if (src == null)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                Debug.LogWarning("[CSVLoader] resourcePath가 비어있고 csvAsset도 없습니다.");
                pattern.Clear();
                return false;
            }

            src = Resources.Load<TextAsset>(resourcePath);
            if (src == null)
            {
                Debug.LogError($"[CSVLoader] Resources.Load 실패: \"{resourcePath}\" (확장자 제외)");
                pattern.Clear();
                return false;
            }
        }

        // 2) 텍스트를 라인 단위로 읽기 위한 StringReader 준비
        using (var reader = new StringReader(src.text))
        {
            pattern.Clear();

            // 3) 첫 줄(헤더) 읽기
            string headerLine = reader.ReadLine();
            if (headerLine == null)
            {
                Debug.LogError("[CSVLoader] CSV가 비어 있습니다(헤더 없음).");
                return false;
            }

            // 4) 구분자 자동 감지 (콤마/탭/세미콜론)
            char sep = DetectSeparator(headerLine);

            // 5) 헤더에서 각 열 인덱스 찾기 (대소문자 무시)
            if (!TryParseHeader(headerLine, sep, out int beatIdx, out int xIdx, out int yIdx))
            {
                Debug.LogError("[CSVLoader] 헤더에 'beat', 'x', 'y'가 모두 포함되어야 합니다.");
                return false;
            }

            // 6) 본문 각 줄 파싱
            string line;
            int lineNumber = 1; // 헤더 = 1행
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                // 공백/빈 줄/주석(#)은 건너뜀
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (IsComment(line)) continue;

                string[] cols = line.Split(sep);
                TrimAll(cols); // 각 셀 공백 제거

                // 필요한 열 수 검사
                int needed = Mathf.Max(beatIdx, xIdx, yIdx);
                if (cols.Length <= needed)
                {
                    Debug.LogWarning($"[CSVLoader] {lineNumber}행: 열 개수 부족 → 건너뜀");
                    continue;
                }

                // '.' 소수점 기준으로 파싱 (로케일 영향 제거)
                if (!float.TryParse(cols[beatIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out float beat))
                {
                    Debug.LogWarning($"[CSVLoader] {lineNumber}행: beat 파싱 실패 → '{cols[beatIdx]}'");
                    continue;
                }
                if (!float.TryParse(cols[xIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                {
                    Debug.LogWarning($"[CSVLoader] {lineNumber}행: x 파싱 실패 → '{cols[xIdx]}'");
                    continue;
                }
                if (!float.TryParse(cols[yIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                {
                    Debug.LogWarning($"[CSVLoader] {lineNumber}행: y 파싱 실패 → '{cols[yIdx]}'");
                    continue;
                }

                pattern.Add(new NoteEvent(beat, x, y));
            }
        }

        // 7) 스폰 순서를 보장하기 위해 beat 기준 정렬
        pattern.Sort((a, b) => a.beat.CompareTo(b.beat));

        Debug.Log($"[CSVLoader] 로드 완료 (총 {pattern.Count}개).");
        return true;
    }

    // --------- 유틸리티 ---------

    private static char DetectSeparator(string headerLine)
    {
        // 가장 흔한 3가지만 지원: 콤마, 탭, 세미콜론
        if (headerLine.IndexOf(',') >= 0) return ',';
        if (headerLine.IndexOf('\t') >= 0) return '\t';
        if (headerLine.IndexOf(';') >= 0) return ';';
        return ','; // 기본 콤마
    }

    private static bool TryParseHeader(string headerLine, char sep,
                                       out int beatIdx, out int xIdx, out int yIdx)
    {
        beatIdx = xIdx = yIdx = -1;

        string[] heads = headerLine.Split(sep);
        TrimAll(heads);

        for (int i = 0; i < heads.Length; i++)
        {
            string h = heads[i].ToLowerInvariant();
            if (h == "beat") beatIdx = i;
            else if (h == "x") xIdx = i;
            else if (h == "y") yIdx = i;
        }

        return (beatIdx >= 0 && xIdx >= 0 && yIdx >= 0);
    }

    private static bool IsComment(string line)
    {
        // 좌우 공백 무시 후 첫 글자가 '#'이면 주석
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (!char.IsWhiteSpace(c))
                return c == '#';
        }
        return false;
    }

    private static void TrimAll(string[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
            arr[i] = arr[i]?.Trim();
    }
}