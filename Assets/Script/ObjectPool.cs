using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]  //인스펙터 창에 띄우기 용
public class ObjectInfo

{
    public GameObject goPrefab; //풀에 들어갈 프리펩
    public int count; //몇개 생성할지
    public Transform tfPoolParent; //부모 폴더(없으면 pool오브젝트 밑)
}

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    //공유자원 instance를 통해 어디서든 public 변수, 함수에 접근 가능
    [SerializeField] ObjectInfo[] objectInfo = null;

    //여러 종류의 오브젝트를 개별 큐로 관리
    private Queue<GameObject>[] objectQueues;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

    }
    void Start()
    {
        //objectInfo 배열 크기만큼 큐 배열 생성
       objectQueues = new Queue<GameObject>[objectInfo.Length];

        for(int i = 0;i < objectInfo.Length; i++) {
            objectQueues[i] = InsertQueue(objectInfo[i]);
        }

    }


    //특정 풀 생성
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

    // NoteManager에서 쓸 함수 1
    // 단타형에서 쓰는 “noteQueue”는 이제 objectQueues[0]으로 대체 가능
    public GameObject GetNote(int type)
    {
        if(type < 0 || type >= objectQueues.Length)
        {
            Debug.LogError($"[ObjectPool] 잘못된 type 인덱스 요청: {type}");
            return null;
        }

        Queue<GameObject> q = objectQueues[type];

        // 큐를 한 바퀴 돌면서 비활성 오브젝트를 찾아 '꺼내' 사용

        int count = q.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = q.Dequeue(); // 일단 꺼냄
            if (!obj.activeInHierarchy)
            {
                // 사용 중이므로 큐에 '즉시' 되돌리지 않음 (반납 때 다시 Enqueue)
                return obj;
            }
            else
            {
                // 사용 중이면 뒤로 회전
                q.Enqueue(obj);
            }
        }

        // 전부 사용 중이면 새로 생성

        GameObject newObj = Instantiate(objectInfo[type].goPrefab);
        newObj.SetActive(false);
        newObj.transform.SetParent(objectInfo[type].tfPoolParent ?? this.transform);
        return newObj; // 반납 시 Enqueue됨

    }
    //NoteManager에서 쓸 함수 2
    //단타형은 기존처럼 noteQueue.Enqueue() 대신 이렇게 교체 가능
    public void ReturnNote(int type, GameObject note)
    {
        if (type < 0 || type >= objectQueues.Length)
        {
            Debug.LogError($"[ObjectPool] 잘못된 type 인덱스 반환: {type}");
            Destroy(note);
            return;
        }

        note.SetActive(false);
        objectQueues[type].Enqueue(note);//여기서만 큐에 복귀
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