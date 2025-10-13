using UnityEngine;
using UnityEngine.EventSystems;

public class SteeringHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Rotation Settings")]
    public float maxRotation = 30f;   // 최대 회전 각도
    public float returnSpeed = 5f;    // 원위치 복귀 속도

    [Header("Swipe Settings")]
    public float swipeThreshold = 50f; // 스와이프 최소 거리(px)

    private RectTransform rectTransform;
    private float currentRotation = 0f;
    private Vector2 dragStartPos;
    private bool isDragging = false;

    private StageMenu stageMenu;
    [SerializeField] private Animator spinningDisk = null;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        stageMenu = FindObjectOfType<StageMenu>();
    }

    void Update()
    {
        // 드래그 중이 아니면 원위치로 복귀
        if (!isDragging)
        {
            currentRotation = Mathf.Lerp(currentRotation, 0f, Time.deltaTime * returnSpeed);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - dragStartPos;

        bool isRightSide = dragStartPos.x >= Screen.width / 2f;

        // 왼쪽 절반과 오른쪽 절반의 회전 방향 부호 반대
        float direction = isRightSide ? -1f : 1f;

        float tiltAmount = Mathf.Clamp(-delta.y * 0.15f * direction, -maxRotation, maxRotation);
        currentRotation = Mathf.Lerp(currentRotation, tiltAmount, Time.deltaTime * 10f);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        Vector2 dragEndPos = eventData.position;
        Vector2 dragVector = dragEndPos - dragStartPos;

        if (dragVector.magnitude < swipeThreshold)
            return;

        bool isRightSide = dragStartPos.x >= Screen.width / 2f;

        // 좌우 절반별로 위/아래 입력 해석 반전
        if (isRightSide)
        {
            // 오른쪽 절반: 아래로 → 다음곡 / 위로 → 이전곡
            if (dragVector.y < -swipeThreshold)
            {
                OnSteerRight();
                currentRotation = -15f;
            }
            else if (dragVector.y > swipeThreshold)
            {
                OnSteerLeft();
                currentRotation = 15f;
            }
        }
        else
        {
            // 왼쪽 절반: 아래로 → 이전곡 / 위로 → 다음곡
            if (dragVector.y < -swipeThreshold)
            {
                OnSteerLeft();
                currentRotation = -15f;
            }
            else if (dragVector.y > swipeThreshold)
            {
                OnSteerRight();
                currentRotation = 15f;
            }
        }
    }

    private void OnSteerLeft()
    {
        if (spinningDisk != null)
            spinningDisk.SetTrigger("TurnLeft");

        Debug.Log("이전 곡 선택 🎵");
        stageMenu?.BtnPrior();
    }

    private void OnSteerRight()
    {
        if (spinningDisk != null)
            spinningDisk.SetTrigger("TurnRight");

        Debug.Log("다음 곡 선택 🎶");
        stageMenu?.BtnNext();
    }
}
