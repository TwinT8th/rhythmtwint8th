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

    [SerializeField] ObjectInfo[] objectInfo = null;

    private Queue<GameObject>[] objectQueues;
    private List<GameObject>[] allObjects;   // ★ 각 타입별 전체 객체 목록

    void Awake() { instance = this; }

    void Start()
    {
        objectQueues = new Queue<GameObject>[objectInfo.Length];
        allObjects = new List<GameObject>[objectInfo.Length];

        for (int i = 0; i < objectInfo.Length; i++)
        {
            objectQueues[i] = new Queue<GameObject>();
            allObjects[i] = new List<GameObject>();

            // 초기 생성
            var q = InsertQueue(objectInfo[i], i);
            // q는 이미 objectQueues[i]에 들어감
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

            // 풀 타입을 Note/LongNote에 주입(새로 만든 객체도 안전)
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
            Debug.LogError($"[ObjectPool] 잘못된 type 인덱스 요청: {type}");
            return null;
        }

        Queue<GameObject> q = objectQueues[type];

        int count = q.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = q.Dequeue();
            if (!obj.activeInHierarchy)
            {
                // 사용: 큐에 바로 되돌리지 않음 (반납 시에만 Enqueue)
                return obj;
            }
            q.Enqueue(obj);
        }

        // 전부 사용중이면 ‘안전하게’ 추가 생성
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
            Debug.LogError($"[ObjectPool] 잘못된 type 인덱스 반환: {type}");
            Destroy(note);
            return;
        }

        //Note 리셋 보장

        var comp = note.GetComponent<Note>();
        if (comp != null)
            comp.ResetState();

        note.SetActive(false);
        objectQueues[type].Enqueue(note);
    }

    // ★★★ 리플레이/스테이지 종료용: 안전한 전체 리셋
    public void ResetAllPools()
    {
        for (int t = 0; t < objectQueues.Length; t++)
        {
            objectQueues[t].Clear();          // 큐부터 비운다
            foreach (var obj in allObjects[t])
            {
                if (obj == null) continue;
                obj.SetActive(false);         // 전원 비활성
                objectQueues[t].Enqueue(obj); // 다시 큐에 재등록
            }
        }
        Debug.Log("[ObjectPool] ResetAllPools() 완료");
    }
}

// 작은 헬퍼
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