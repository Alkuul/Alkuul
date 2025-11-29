using UnityEngine;

public class Jigger : MonoBehaviour
{
    [Header("Settings")]
    public float smallCapacity = 30f;
    public float largeCapacity = 45f;

    private Vector3 originalPosition;
    private bool isHeld = false;      // 마우스를 따라다니는지 여부
    private bool isFlipped = false;   // 뒤집혔는지 여부 (false: 30ml, true: 45ml)

    // 현재 지거에 담긴 내용물 상태
    private AlcoholData currentContent = null;
    private bool isFilled = false;

    private Camera mainCam;

    void Start()
    {
        originalPosition = transform.position;
        mainCam = Camera.main;
    }

    void Update()
    {
        HandleInput();

        // 지거가 들려있다면 마우스 위치를 따라감
        if (isHeld)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // 2D 평면 유지
            transform.position = mousePos;
        }
    }

    void HandleInput()
    {
        // 1. 우클릭: 지거 회전 및 용량 전환 (180도)
        if (Input.GetMouseButtonDown(1)) // Right Click
        {
            // 지거 위에 마우스가 있거나 들고 있을 때만
            if (IsMouseOver() || isHeld)
            {
                FlipJigger();
            }
        }

        // 2. 좌클릭: 상호작용
        if (Input.GetMouseButtonDown(0)) // Left Click
        {
            if (!isHeld)
            {
                // 바닥에 있을 때 클릭하면 -> 든다
                if (IsMouseOver())
                {
                    isHeld = true;
                }
            }
            else
            {
                // 들고 있는 상태에서의 클릭 처리
                HandleInteractionWhileHeld();
            }
        }
    }

    // 지거 뒤집기
    void FlipJigger()
    {
        // 내용물이 있으면 뒤집을 수 없게 할지, 쏟게 할지 결정 필요 (여기선 비었을 때만 가능하다고 가정)
        if (isFilled)
        {
            Debug.Log("내용물이 들어있어 뒤집을 수 없습니다!");
            return;
        }

        isFlipped = !isFlipped;
        float currentZ = transform.rotation.eulerAngles.z;
        transform.rotation = Quaternion.Euler(0, 0, currentZ + 180f);

        float capacity = isFlipped ? largeCapacity : smallCapacity;
        Debug.Log($"현재 용량: {capacity}ml");
    }

    // 들고 있는 상태에서 클릭했을 때 로직
    // 수정된 함수: HandleInteractionWhileHeld
    void HandleInteractionWhileHeld()
    {
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // 1. 클릭 위치의 모든 물체를 가져옵니다 (Raycast -> RaycastAll 변경)
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach (var hit in hits)
        {
            // 2. 감지된 물체가 '지거(나 자신)'라면 무시하고 넘어갑니다.
            if (hit.collider.gameObject == this.gameObject) continue;

            // 3. 술병을 클릭했다면? -> 술 담기
            if (hit.collider.TryGetComponent(out Bottle bottle))
            {
                FillJigger(bottle.alcoholData);
                return;
            }

            // 4. 믹싱글라스를 클릭했다면? -> 술 따르기
            if (hit.collider.TryGetComponent(out MixingGlass glass))
            {
                PourIntoGlass(glass);
                return;
            }
        }

        // 5. 아무 상호작용도 못 찾았을 때만 제자리로 복귀
        ReturnToOriginalPosition();
    }

    void FillJigger(AlcoholData data)
    {
        if (isFilled)
        {
            Debug.Log("이미 지거가 차 있습니다.");
            return;
        }

        currentContent = data;
        isFilled = true;
        float capacity = isFlipped ? largeCapacity : smallCapacity;
        Debug.Log($"{data.alcoholName}을(를) {capacity}ml 담았습니다.");

        // 시각적 효과 추가 가능 (예: 지거 색상 변경)
        GetComponent<SpriteRenderer>().color = Color.yellow; // 임시 피드백
    }

    void PourIntoGlass(MixingGlass glass)
    {
        if (!isFilled)
        {
            Debug.Log("지거가 비어있습니다.");
            return;
        }

        float capacity = isFlipped ? largeCapacity : smallCapacity;
        glass.AddLiquid(currentContent, capacity);

        // 비우기
        currentContent = null;
        isFilled = false;
        GetComponent<SpriteRenderer>().color = Color.white; // 색상 복구
    }

    void ReturnToOriginalPosition()
    {
        isHeld = false;
        transform.position = originalPosition;
    }

    // 마우스가 현재 지거 위에 있는지 확인하는 헬퍼 함수
    bool IsMouseOver()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
        return hitCollider != null && hitCollider.gameObject == this.gameObject;
    }
}