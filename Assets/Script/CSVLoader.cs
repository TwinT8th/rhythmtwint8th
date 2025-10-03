// ���ϸ�: CSVLoader.cs  (�ݵ�� �� �̸��� Ŭ�������� ��ġ�ؾ� ��)

using System;
using System.Collections.Generic;
using System.Globalization;   // �Ҽ��� �Ľ��� '.'�� �����ϱ� ����
using System.IO;              // StringReader ���
using UnityEngine;

/// <summary>
/// CSV �� �� = Ư�� ����(beat)�� Ư�� ��ǥ(x, y)�� ��Ʈ�� �������� �ǹ�
/// </summary>
[Serializable]
public struct NoteEvent
{
    public float beat;    // �� ���� �������� (0.0���� ���� ����)
    public float x;       // ���� X ��ǥ (����/���� � ��ǥ�������� �����ʿ��� ����)
    public float y;       // ���� Y ��ǥ

    public NoteEvent(float beat, float x, float y)
    {
        this.beat = beat;
        this.x = x;
        this.y = y;
    }

    public Vector2 Position => new Vector2(x, y); // ���ǿ�
}

/// <summary>
/// Resources�� CSV(TextAsset)�� �о� List<NoteEvent>�� ��ȯ�ϴ� �δ�.
/// - CSV ���: beat, x, y (��ҹ��� ����)
/// - �� ��, # �ּ� ���� ����
/// - ������: �޸�/�����ݷ�/�� �ڵ� ����
/// - �Ҽ���: �׻� '.' ���� ǥ��(, ��� ����)
/// </summary>
public class CSVLoader : MonoBehaviour
{
    [Header("�Է� �ҽ�(�� �� �ϳ� ���)")]
    [Tooltip("Resources ���� ���(Ȯ���� ����). ��: \"pattern0\" �Ǵ� \"Patterns/pattern0\"")]
    public string resourcePath = "pattern0";

    [Tooltip("���� TextAsset�� �巡���� �־ ��� ���� (���ҽ� ��κ��� �켱)")]
    public TextAsset csvAsset;

    [Header("��� ������ (�б� �뵵�� ���)")]
    [SerializeField] private List<NoteEvent> pattern = new List<NoteEvent>();

    /// <summary>
    /// �ٸ� ��ũ��Ʈ���� ���� �� ���. (������ �״�� �ѱ�Ƿ� ���� ����)
    /// </summary>
    public List<NoteEvent> GetPattern() => pattern;

    private void Awake()
    {
        // ���� �� �ڵ� �ε�. �����ϸ� �ֿܼ� ���� ���.
        if (!TryLoad())
        {
            Debug.LogError("[CSVLoader] CSV �ε忡 �����߽��ϴ�. ��γ� ������ ������ Ȯ���ϼ���.");
        }
    }

    /// <summary>
    /// ������ csvAsset �Ǵ� resourcePath�� ����� CSV�� �о� pattern�� ����.
    /// </summary>
    public bool TryLoad()
    {
        // 1) �Է� �ҽ� ����: �ν����Ϳ� TextAsset�� ������ �װ� �켱 ���
        TextAsset src = csvAsset;
        if (src == null)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                Debug.LogWarning("[CSVLoader] resourcePath�� ����ְ� csvAsset�� �����ϴ�.");
                pattern.Clear();
                return false;
            }

            src = Resources.Load<TextAsset>(resourcePath);
            if (src == null)
            {
                Debug.LogError($"[CSVLoader] Resources.Load ����: \"{resourcePath}\" (Ȯ���� ����)");
                pattern.Clear();
                return false;
            }
        }

        // 2) �ؽ�Ʈ�� ���� ������ �б� ���� StringReader �غ�
        using (var reader = new StringReader(src.text))
        {
            pattern.Clear();

            // 3) ù ��(���) �б�
            string headerLine = reader.ReadLine();
            if (headerLine == null)
            {
                Debug.LogError("[CSVLoader] CSV�� ��� �ֽ��ϴ�(��� ����).");
                return false;
            }

            // 4) ������ �ڵ� ���� (�޸�/��/�����ݷ�)
            char sep = DetectSeparator(headerLine);

            // 5) ������� �� �� �ε��� ã�� (��ҹ��� ����)
            if (!TryParseHeader(headerLine, sep, out int beatIdx, out int xIdx, out int yIdx))
            {
                Debug.LogError("[CSVLoader] ����� 'beat', 'x', 'y'�� ��� ���ԵǾ�� �մϴ�.");
                return false;
            }

            // 6) ���� �� �� �Ľ�
            string line;
            int lineNumber = 1; // ��� = 1��
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                // ����/�� ��/�ּ�(#)�� �ǳʶ�
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (IsComment(line)) continue;

                string[] cols = line.Split(sep);
                TrimAll(cols); // �� �� ���� ����

                // �ʿ��� �� �� �˻�
                int needed = Mathf.Max(beatIdx, xIdx, yIdx);
                if (cols.Length <= needed)
                {
                    Debug.LogWarning($"[CSVLoader] {lineNumber}��: �� ���� ���� �� �ǳʶ�");
                    continue;
                }

                // '.' �Ҽ��� �������� �Ľ� (������ ���� ����)
                if (!float.TryParse(cols[beatIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out float beat))
                {
                    Debug.LogWarning($"[CSVLoader] {lineNumber}��: beat �Ľ� ���� �� '{cols[beatIdx]}'");
                    continue;
                }
                if (!float.TryParse(cols[xIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                {
                    Debug.LogWarning($"[CSVLoader] {lineNumber}��: x �Ľ� ���� �� '{cols[xIdx]}'");
                    continue;
                }
                if (!float.TryParse(cols[yIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                {
                    Debug.LogWarning($"[CSVLoader] {lineNumber}��: y �Ľ� ���� �� '{cols[yIdx]}'");
                    continue;
                }

                pattern.Add(new NoteEvent(beat, x, y));
            }
        }

        // 7) ���� ������ �����ϱ� ���� beat ���� ����
        pattern.Sort((a, b) => a.beat.CompareTo(b.beat));

        Debug.Log($"[CSVLoader] �ε� �Ϸ� (�� {pattern.Count}��).");
        return true;
    }

    // --------- ��ƿ��Ƽ ---------

    private static char DetectSeparator(string headerLine)
    {
        // ���� ���� 3������ ����: �޸�, ��, �����ݷ�
        if (headerLine.IndexOf(',') >= 0) return ',';
        if (headerLine.IndexOf('\t') >= 0) return '\t';
        if (headerLine.IndexOf(';') >= 0) return ';';
        return ','; // �⺻ �޸�
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
        // �¿� ���� ���� �� ù ���ڰ� '#'�̸� �ּ�
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