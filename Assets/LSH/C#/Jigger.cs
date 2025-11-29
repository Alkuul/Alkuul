using UnityEngine;

public class Jigger : MonoBehaviour
{
    [Header("Settings")]
    public float smallCapacity = 30f;
    public float largeCapacity = 45f;

    private Vector3 originalPosition;
    private bool isHeld = false;
    private bool isFlipped = false;   // false: 30ml, true: 45ml

    private AlcoholData currentContent = null; // 현재 내용물
    private bool isFilled = false;             // 내용물이 찼는지 여부

    private Camera mainCam;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    void Start()
    {
        originalPosition = transform.position;
        mainCam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer.sortingOrder;
    }

    void Update()
    {
        // 들고 있으면 따라다니기
        if (isHeld)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos;
        }

        HandleInput();
    }

    void HandleInput()
    {
        // 1. 우클릭: 지거 회전 (용량 전환) - 들고 있지 않을 때만 가능하게 설정
        if (Input.GetMouseButtonDown(1) && !isHeld)
        {
            if (IsMouseOver())
            {
                FlipJigger();
            }
        }

        // 2. 좌클릭: 지거 집기 or 믹싱글라스에 붓기
        if (Input.GetMouseButtonDown(0))
        {
            if (!isHeld)
            {
                // 바닥에 있을 때: 내용물이 차 있어야만 들 수 있음!
                if (IsMouseOver())
                {
                    if (isFilled)
                    {
                        isHeld = true;
                        spriteRenderer.sortingOrder = 100; // 맨 앞으로
                    }
                    else
                    {
                        Debug.Log("지거가 비어있어서 들 수 없습니다. 먼저 술을 채워주세요.");
                    }
                }
            }
            else
            {
                // 들고 있을 때: 믹싱글라스 찾기
                HandleInteractionWhileHeld();
            }
        }
    }

    // 술병(Bottle) 스크립트에서 호출하는 함수
    public bool FillFromBottle(AlcoholData data)
    {
        if (isFilled)
        {
            Debug.Log("지거가 이미 차 있습니다! 버리거나 믹싱글라스에 넣으세요.");
            return false; // 이미 차있으면 실패 반환
        }

        currentContent = data;
        isFilled = true;

        float currentCap = isFlipped ? largeCapacity : smallCapacity;
        Debug.Log($"지거({currentCap}ml)에 {data.alcoholName} 채워짐!");

        // 시각적 피드백 (노란색으로 변경)
        spriteRenderer.color = Color.yellow;
        return true;
    }

    void HandleInteractionWhileHeld()
    {
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == this.gameObject) continue;

            // 믹싱글라스를 클릭했다면? -> 붓기
            if (hit.collider.TryGetComponent(out MixingGlass glass))
            {
                PourIntoGlass(glass);
                return;
            }
        }

        // 아무것도 클릭 안 했으면 제자리로
        ReturnToOriginalPosition();
    }

    void PourIntoGlass(MixingGlass glass)
    {
        float capacity = isFlipped ? largeCapacity : smallCapacity;
        glass.AddLiquid(currentContent, capacity);

        // 비우기 및 초기화
        currentContent = null;
        isFilled = false;
        spriteRenderer.color = Color.white; // 색상 복구

        ReturnToOriginalPosition(); // 붓고 나면 자동으로 제자리로
    }

    void FlipJigger()
    {
        if (isFilled)
        {
            Debug.Log("술이 들어있어 뒤집을 수 없습니다.");
            return;
        }

        isFlipped = !isFlipped;
        transform.Rotate(0, 0, 180); // 시각적 회전
        float cap = isFlipped ? largeCapacity : smallCapacity;
        Debug.Log($"지거 용량 변경 -> {cap}ml");
    }

    void ReturnToOriginalPosition()
    {
        isHeld = false;
        transform.position = originalPosition;
        spriteRenderer.sortingOrder = originalSortingOrder;
    }

    bool IsMouseOver()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
        return hitCollider != null && hitCollider.gameObject == this.gameObject;
    }
}