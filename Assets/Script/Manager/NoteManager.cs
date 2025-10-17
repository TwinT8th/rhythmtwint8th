using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


// �� ��ü�� BPM ��ȭ �����ؾ� ��.

public class NoteManager : MonoBehaviour
{
    public static NoteManager instance;

    [Header("����, ����")]
    //public AudioSource music; //BGM0 AudioSource �Ҵ�
    public float bpm = 90f; //1�д� ��Ʈ �� �� ���� - �� ��ü�� �޾ƿ���.
    public double songStartOffsetSec = 0d; //���� ���� ����(��), �ʿ� ������ 0
    public CSVLoader csvLoader;

    [Header("����")]
    [SerializeField] Transform noteParent = null; // ��Ʈ���� ���� �θ� ������Ʈ


    private List<NoteEvent> pattern; //CSV���� ���� ������
    private List<double> spawnTimesSec;     //�� ��Ʈ�� ���� �����ð�(��)
    private List<double> targetTimesSec; //�߰�: Ÿ�� �ð�(��, ����� �ϴ� ����)
    private int nextIndex = 0; //���� ������ ��Ʈ �ε���

    [Header("DSP Schedule")]
    [SerializeField] double leadInSec = 1.0d; // ����� ���� ���� �ð�(��)
    private double scheduledStartDSPTime; // ������� ���۵� DSP �ð�(����)

    [Header("Approach Settings")]
    [SerializeField] public float approachBeats = 2f; // �߰�: Ÿ�ٺ��� �� ���� ���� ��Ÿ���� (Ÿ�ּ̹�Ŭ/��Ʈ��Ŀ ���̿� ����)

    public double lastNoteTimeSec = 0; // ������ ��Ʈ�� �ð�
    private bool isLastNoteProcessed = false;
    int songIndex = 0;

//    StageVideoController stageVideo;


    void Awake()
    {
        instance = this;

    }


    void Start()
    {
   //     stageVideo = FindObjectOfType<StageVideoController>();
    }

    // Update is called once per frame
    void Update()
    {



        if (GameManager.instance.isStartGame)
        {

            if (AudioManager.instance == null) return;
            if (pattern == null || pattern.Count == 0) return;

            //���뷡 ���� ���� 0�ʡ��� ������ ���� �ð�(��). �� ���� �뷡 ���� ���� 0�� �ǵ��� �����
            double nowSec = AudioSettings.dspTime - scheduledStartDSPTime;

            //offset�� ���� "���� ���� 0��"�� ����
            nowSec -= songStartOffsetSec;


            //���� �����ӿ� ���� ��Ʈ�� ����̸� ��� ó��. ���� �������� �� (Ÿ���� �ƴ�)
            while (nextIndex < spawnTimesSec.Count && nowSec >= spawnTimesSec[nextIndex])
            {
                SpawnNote(pattern[nextIndex], nextIndex);
                nextIndex++;
            }
        }


    }

    public void StartMusic()
    {

        // GameManager���� ���� ���õ� �� �ε��� �޾ƿ���
        songIndex = GameManager.instance.currentSongIndex;


        //CSV���� �ε�

        pattern = csvLoader.GetPattern(songIndex);
        if(pattern == null || pattern.Count == 0)
        {
            Debug.LogError($"[NoteManager] pattern{songIndex}.csv �ε� ����!");
            return;
        }

        // 2) ����Ÿ�� ���
        PrecomputeSpawnTimes();
        nextIndex = 0;

        // ����� ���� ����: ���� DSP �ð� + leadInSec �� ��Ȯ�� ����
        if (AudioManager.instance != null)
        {
            scheduledStartDSPTime = AudioSettings.dspTime + leadInSec;

            // Inspector���� bgm ����Ʈ �ȿ� �� �̸� ����־� ��. �������� �Ŵ��� �����ϸ鼭 ����
            AudioManager.instance.PlayBGM("BGM" + songIndex, leadInSec);
            Debug.Log($"[NoteManager] BGM{songIndex} + pattern{songIndex}.csv ���� ����");

        }

        else
        {
            Debug.LogWarning("[NoteManager] AudioManager instance�� �����ϴ�. ���� ��� �Ұ�");
        }

    }

    private void LoadPattern(string fileName)
    {
        TextAsset patternFile = Resources.Load<TextAsset>(fileName);
        if (patternFile == null)
        {
            Debug.LogError($"[NoteManager] ���� ���� '{fileName}.csv'��(��) ã�� �� �����ϴ�!");
            return;
        }
    }


    private void PrecomputeSpawnTimes() //CSV���� ������ beat�� ���� ����ð�(��)�� �ѹ��� ����صδ� ����
    {
        spawnTimesSec = new List<double>(pattern.Count);
        targetTimesSec = new List<double>(pattern.Count); 

        double secPerBeat = 60 / bpm;
        double approachSec = approachBeats * secPerBeat;  //Ÿ�ٺ��� �̸� ���� �ð�(��)
        
        lastNoteTimeSec = 0; // �ʱ�ȭ

        for (int i = 0; i < pattern.Count; i++)
        {
            //  @ Ÿ�� �ð�(���� ���� ����ð�, ��) (@���� �̰ſ���)�� ��Ʈ�� ���� �����ð�(��) = ������ + beat * (60/BPM)
            double target = (double)songStartOffsetSec + (double)pattern[i].beat * secPerBeat;
            targetTimesSec.Add(target);

            double spawn = target - approachSec; // @ ���� �ð� = Ÿ�� �ð� - ���� �ð�
            spawnTimesSec.Add(spawn);

            // ������ ��Ʈ �ð� ����
            if (target > lastNoteTimeSec)
                lastNoteTimeSec = target;

            // Debug.Log($"[SpawnTable] beat={pattern[i].beat}, spawn={spawn:F3}s, target={target:F3}s");
        }

        Debug.Log($"[NoteManager] Last Note Time = {lastNoteTimeSec:F3}s");
    }



    /*
  SpawnNote:
  - ������ ���̸��� �ϰ�,
  - Note.Init���� ��Ÿ���� ���� DSP �ð����� �־���.
*/
    private void SpawnNote(NoteEvent e, int index)
    {
       //Ǯ���� ������Ʈ ��������
        GameObject note = ObjectPool.instance.noteQueue.Dequeue();

        // ��ǥ�� ����: ���⼭�� �θ� ���� localPosition���� ��ġ
        note.transform.SetParent(noteParent); //�θ� ����
        note.transform.localPosition = new Vector3(e.x, e.y, 0f);
        note.transform.localScale = Vector3.one;
        note.SetActive(true);

        // Note ������Ʈ �ʱ�ȭ
        Note comp = note.GetComponent<Note>();
        if (comp != null)
        {
            //  @ Ÿ���� ���� DSP �ð� = ����� ���� ���� DSP + Ÿ��(��) 
            double targetDSPTime = scheduledStartDSPTime + targetTimesSec[index];
            comp.Init(targetDSPTime);   // Ÿ�� �����롱 �ð� ���� (Note�� �̰� �������� ����)
        }

    }
    public void ReturnNote(Note note)
    {

        note.gameObject.SetActive(false);
        ObjectPool.instance.noteQueue.Enqueue(note.gameObject);

        // ������ ��Ʈ�� ó���Ǿ����� ����
        if (!isLastNoteProcessed && Mathf.Abs((float)(note.targetTimeSec - (scheduledStartDSPTime + lastNoteTimeSec))) < 0.02f)
        {
            isLastNoteProcessed = true;
            Debug.Log("[NoteManager] ������ ��Ʈ ���� �Ϸ�!");

            StartCoroutine(ShowResultAfterFade());

            //StageManager.instance.StartEndSequence(); - ���� �������� �Ŵ��� ���� ����
        }

    }
    private IEnumerator ShowResultAfterFade()
    {
        float fadeDuration = 3f;

        // ����� ���̵� �ƿ�
        if (AudioManager.instance != null)
        {
            yield return StartCoroutine(AudioManager.instance.FadeOutBGM(fadeDuration));
        }
        // ���� �Ͻ�����
        StageVideoController stageVideo = FindObjectOfType<StageVideoController>();
        if (stageVideo != null)
        {
            stageVideo.PauseVideo();   // ������ ����
                                      // �Ǵ� stageVideo.PauseVideo(); (�Ͻ������� �ϰ� �ʹٸ�)
        }
        // ���̵� �ð� ���
        yield return new WaitForSeconds(fadeDuration);

        // ���â ǥ��
        Result resultUI = FindObjectOfType<Result>();
        if (resultUI != null)
        {
            resultUI.ShowResult();
        }
    }

    //���Ͽ� ��ϵ� �� ��Ʈ ��
    public int GetTotalNoteCount()
    {
        return (pattern != null) ? pattern.Count : 0;
    }

    public void ResetForReplay()
    {
        //Debug.Log("[NoteManager] ResetForReplay() ȣ��� - ���� �ʱ�ȭ ����");

        nextIndex = 0;
        isLastNoteProcessed = false;

        // ���� �ٽ� �ε� (CSVLoader���� �ٽ� �б�)
        pattern = csvLoader.GetPattern(songIndex);
        PrecomputeSpawnTimes();

        // ��� ��Ʈ ������Ʈ ��Ȱ��ȭ (Ȥ�� �����ִ� �� ������)
        foreach (Transform child in noteParent)
            child.gameObject.SetActive(false);

        // ������� ����
        AudioManager.instance.StopBGM();

    }

}


