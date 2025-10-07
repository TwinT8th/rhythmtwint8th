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
    public AudioSource music; //BGM0 AudioSource �Ҵ�
    public float bpm = 90f; //1�д� ��Ʈ �� �� ���� - �� ��ü�� �޾ƿ���.
    public double songStartOffsetSec = 0d; //���� ���� ����(��), �ʿ� ������ 0
    //double currentTime = 0d; //��Ʈ ������ ���� �ð��� üũ�� ����
    public CSVLoader csvLoader;

    [Header("����")]
    [SerializeField] Transform noteParent = null; // ��Ʈ���� ���� �θ� ������Ʈ
    //[SerializeField] GameObject notePrefab = null; //��Ʈ �������� ���� ����

    private List<NoteEvent> pattern; //CSV���� ���� ������
    private List<double> spawnTimesSec;     //�� ��Ʈ�� ���� �����ð�(��)
    private List<double> targetTimesSec; //�߰�: Ÿ�� �ð�(��, ����� �ϴ� ����)
    private int nextIndex = 0; //���� ������ ��Ʈ �ε���

    [Header("DSP Schedule")]
    [SerializeField] double leadInSec = 1.0d; // ����� ���� ���� �ð�(��)
    private double scheduledStartDSPTime; // ������� ���۵� DSP �ð�(����)

    [Header("Approach Settings")]
    [SerializeField] public float approachBeats = 2f; // �߰�: Ÿ�ٺ��� �� ���� ���� ��Ÿ���� (Ÿ�ּ̹�Ŭ/��Ʈ��Ŀ ���̿� ����)

    void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        //CSV���� ���� ��������
        pattern = csvLoader.GetPattern();
        PrecomputeSpawnTimes();// @ ����/Ÿ�� �ð� ��� ���
        nextIndex = 0;

        // 2) ����� ���� ����: ���� DSP �ð� + leadInSec �� ��Ȯ�� ����
        if (music != null && music.clip != null)
        {
            //PlayOnAwake�� �����־�� �� (Inspector)
            scheduledStartDSPTime = AudioSettings.dspTime + leadInSec;
            music.PlayScheduled(scheduledStartDSPTime);
        }

    }


    // Update is called once per frame
    void Update()
    {

        if (music == null || music.clip == null) return;
        if (pattern == null || pattern.Count == 0) return;

        //���뷡 ���� ���� 0�ʡ��� ������ ���� �ð�(��). �� ���� �뷡 ���� ���� 0�� �ǵ��� �����
        double nowSec = AudioSettings.dspTime - scheduledStartDSPTime; 

        //offset�� ���� "���� ���� 0��"�� ����
        nowSec -= songStartOffsetSec;

        //���� �����ӿ� ���� ��Ʈ�� ����̸� ��� ó��. ���� �������� �� (Ÿ���� �ƴ�)
        while (nextIndex<spawnTimesSec.Count&& nowSec >= spawnTimesSec[nextIndex])
            {
            SpawnNote(pattern[nextIndex], nextIndex);
            nextIndex++;
            }

    }

    private void PrecomputeSpawnTimes() //CSV���� ������ beat�� ���� ����ð�(��)�� �ѹ��� ����صδ� ����
    {
        spawnTimesSec = new List<double>(pattern.Count);
        targetTimesSec = new List<double>(pattern.Count); // @ �߰�

        double secPerBeat = 60 / bpm;
        double approachSec = approachBeats * secPerBeat;  // @ Ÿ�ٺ��� �̸� ���� �ð�(��)

        for (int i = 0; i < pattern.Count; i++)
        {
            //  @ Ÿ�� �ð�(���� ���� ����ð�, ��) (@���� �̰ſ���)�� ��Ʈ�� ���� �����ð�(��) = ������ + beat * (60/BPM)
            double target = (double)songStartOffsetSec + (double)pattern[i].beat * secPerBeat;
            targetTimesSec.Add(target);

            double spawn = target - approachSec; // @ ���� �ð� = Ÿ�� �ð� - ���� �ð�
            spawnTimesSec.Add(spawn);
            Debug.Log($"[SpawnTable] beat={pattern[i].beat}, spawn={spawn:F3}s, target={target:F3}s");
        }
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
    }

    // �ʿ��ϴٸ� Note ������Ʈ�� ��ǥ beat/�ð� ����(����/Ÿ�ֿ̹�)
    // var comp = note.GetComponent<Note>();
    // if (comp != null) comp.Init(targetBeat: e.beat, bpm: bpm);
}


