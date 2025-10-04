using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]  //인스펙터 창에 띄우기 용
public class ObjectInfo

{
    public GameObject goPrefab;
    public int count;
    public Transform tfPoolParent;
}

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    //공유자원 instance를 통해 어디서든 public 변수, 함수에 접근 가능
    [SerializeField] ObjectInfo[] objectInfo = null;

    public Queue<GameObject> noteQueue = new Queue<GameObject>();
    //Queue : 선입선출 자료형(가장 먼저 들어간 데이터가 가장 먼저 나옴)

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        noteQueue = InsertQueue(objectInfo[0]);

    }

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


}
