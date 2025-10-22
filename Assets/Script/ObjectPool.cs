using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]  //�ν����� â�� ���� ��
public class ObjectInfo

{
    public GameObject goPrefab; //Ǯ�� �� ������
    public int count; //� ��������
    public Transform tfPoolParent; //�θ� ����(������ pool������Ʈ ��)
}

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField] ObjectInfo[] objectInfo = null;

    private Queue<GameObject>[] objectQueues;
    private List<GameObject>[] allObjects;   // �� �� Ÿ�Ժ� ��ü ��ü ���

    void Awake() { instance = this; }

    void Start()
    {
        objectQueues = new Queue<GameObject>[objectInfo.Length];
        allObjects = new List<GameObject>[objectInfo.Length];

        for (int i = 0; i < objectInfo.Length; i++)
        {
            objectQueues[i] = new Queue<GameObject>();
            allObjects[i] = new List<GameObject>();

            // �ʱ� ����
            var q = InsertQueue(objectInfo[i], i);
            // q�� �̹� objectQueues[i]�� ��
        }
    }

    private Queue<GameObject> InsertQueue(ObjectInfo info, int type)
    {
        Queue<GameObject> q = objectQueues[type];

        for (int i = 0; i < info.count; i++)
        {
            GameObject clone = Instantiate(info.goPrefab, transform.position, Quaternion.identity);
            clone.SetActive(false);
            (info.tfPoolParent ?? this.transform).TrySetParent(clone.transform);

            // Ǯ Ÿ���� Note/LongNote�� ����(���� ���� ��ü�� ����)
            var note = clone.GetComponent<Note>();
            if (note) note.poolType = type;
            var lnote = clone.GetComponent<LongNote>();
            if (lnote) lnote.poolType = type;

            q.Enqueue(clone);
            allObjects[type].Add(clone);
        }
        return q;
    }

    public GameObject GetNote(int type)
    {
        if (type < 0 || type >= objectQueues.Length)
        {
            Debug.LogError($"[ObjectPool] �߸��� type �ε��� ��û: {type}");
            return null;
        }

        Queue<GameObject> q = objectQueues[type];

        int count = q.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = q.Dequeue();
            if (!obj.activeInHierarchy)
            {
                // ���: ť�� �ٷ� �ǵ����� ���� (�ݳ� �ÿ��� Enqueue)
                return obj;
            }
            q.Enqueue(obj);
        }

        // ���� ������̸� �������ϰԡ� �߰� ����
        GameObject newObj = Instantiate(objectInfo[type].goPrefab);
        newObj.SetActive(false);
        (objectInfo[type].tfPoolParent ?? this.transform).TrySetParent(newObj.transform);

        var note = newObj.GetComponent<Note>();
        if (note) note.poolType = type;
        var lnote = newObj.GetComponent<LongNote>();
        if (lnote) lnote.poolType = type;

        allObjects[type].Add(newObj);
        return newObj;
    }

    public void ReturnNote(int type, GameObject note)
    {
        if (type < 0 || type >= objectQueues.Length)
        {
            Debug.LogError($"[ObjectPool] �߸��� type �ε��� ��ȯ: {type}");
            Destroy(note);
            return;
        }

        //Note ���� ����

        var comp = note.GetComponent<Note>();
        if (comp != null)
            comp.ResetState();

        note.SetActive(false);
        objectQueues[type].Enqueue(note);
    }

    // �ڡڡ� ���÷���/�������� �����: ������ ��ü ����
    public void ResetAllPools()
    {
        for (int t = 0; t < objectQueues.Length; t++)
        {
            objectQueues[t].Clear();          // ť���� ����
            foreach (var obj in allObjects[t])
            {
                if (obj == null) continue;
                obj.SetActive(false);         // ���� ��Ȱ��
                objectQueues[t].Enqueue(obj); // �ٽ� ť�� ����
            }
        }
        Debug.Log("[ObjectPool] ResetAllPools() �Ϸ�");
    }
}

// ���� ����
static class TransformExt
{
    public static void TrySetParent(this Transform parent, Transform child)
    {
        if (parent != null && child != null)
            child.SetParent(parent);
    }
}

/*
Queue<GameObject> InsertQueue(ObjectInfo p_objectInfo)
{
    Queue<GameObject> t_queue = new Queue<GameObject>();
    for (int i = 0; i < p_objectInfo.count; i++)
    {
        GameObject t_clone = Instantiate(p_objectInfo.goPrefab, transform.position, Quaternion.identity);
        t_clone.SetActive(false);
        if (p_objectInfo.tfPoolParent != null)
            t_clone.transform.SetParent(p_objectInfo.tfPoolParent);
        else
            t_clone.transform.SetParent(this.transform);
        t_queue.Enqueue(t_clone);
    }
    return t_queue;


}

*/