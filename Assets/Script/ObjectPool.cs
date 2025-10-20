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

    //�����ڿ� instance�� ���� ��𼭵� public ����, �Լ��� ���� ����
    [SerializeField] ObjectInfo[] objectInfo = null;

    //���� ������ ������Ʈ�� ���� ť�� ����
    private Queue<GameObject>[] objectQueues;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

    }
    void Start()
    {
        //objectInfo �迭 ũ�⸸ŭ ť �迭 ����
       objectQueues = new Queue<GameObject>[objectInfo.Length];

        for(int i = 0;i < objectInfo.Length; i++) {
            objectQueues[i] = InsertQueue(objectInfo[i]);
        }

    }


    //Ư�� Ǯ ����
private Queue<GameObject> InsertQueue(ObjectInfo info)
    {
        Queue<GameObject> t_queue = new Queue<GameObject>();

        for (int i = 0; i < info.count; i++)
        {
            GameObject clone = Instantiate(info.goPrefab, transform.position, Quaternion.identity);
            clone.SetActive(false);

            if (info.tfPoolParent != null)
                clone.transform.SetParent(info.tfPoolParent);
            else
                clone.transform.SetParent(this.transform);

            t_queue.Enqueue(clone);
        }

        return t_queue;

    }

    // NoteManager���� �� �Լ� 1
    // ��Ÿ������ ���� ��noteQueue���� ���� objectQueues[0]���� ��ü ����
    public GameObject GetNote(int type)
    {
        if(type < 0 || type >= objectQueues.Length)
        {
            Debug.LogError($"[ObjectPool] �߸��� type �ε��� ��û: {type}");
            return null;
        }

        Queue<GameObject> q = objectQueues[type];

        // ť�� �� ���� ���鼭 ��Ȱ�� ������Ʈ�� ã�� '����' ���

        int count = q.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = q.Dequeue(); // �ϴ� ����
            if (!obj.activeInHierarchy)
            {
                // ��� ���̹Ƿ� ť�� '���' �ǵ����� ���� (�ݳ� �� �ٽ� Enqueue)
                return obj;
            }
            else
            {
                // ��� ���̸� �ڷ� ȸ��
                q.Enqueue(obj);
            }
        }

        // ���� ��� ���̸� ���� ����

        GameObject newObj = Instantiate(objectInfo[type].goPrefab);
        newObj.SetActive(false);
        newObj.transform.SetParent(objectInfo[type].tfPoolParent ?? this.transform);
        return newObj; // �ݳ� �� Enqueue��

    }
    //NoteManager���� �� �Լ� 2
    //��Ÿ���� ����ó�� noteQueue.Enqueue() ��� �̷��� ��ü ����
    public void ReturnNote(int type, GameObject note)
    {
        if (type < 0 || type >= objectQueues.Length)
        {
            Debug.LogError($"[ObjectPool] �߸��� type �ε��� ��ȯ: {type}");
            Destroy(note);
            return;
        }

        note.SetActive(false);
        objectQueues[type].Enqueue(note);//���⼭�� ť�� ����
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