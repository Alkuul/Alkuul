using UnityEngine;

public class Bottle : MonoBehaviour
{
    public AlcoholData alcoholData; // 술 데이터

    private Vector3 originalPosition;
    private bool isHeld = false;
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
        // 좌클릭
        if (Input.GetMouseButtonDown(0))
        {
            if (isHeld)
            {
                // 2. 이미 들고 있다면 -> 상호작용 시도 (따르기 or 내려놓기)
                HandleInteraction();
            }
            else
            {
                // 1. 들고 있지 않다면 -> 마우스가 술병 위에 있는지 확인 후 집기
                if (IsMouseOver())
                {
                    PickUp();
                }
            }
        }

        // 들고 있는 상태라면 위치 갱신
        if (isHeld)
        {
            MoveWithMouse();
        }
    }

    void PickUp()
    {
        isHeld = true;
        spriteRenderer.sortingOrder = 100; // 맨 앞으로 가져오기
    }

    void MoveWithMouse()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;
    }

    void HandleInteraction()
    {
        // 마우스 위치에 무엇이 있는지 확인
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach (var hit in hits)
        {
            // 자기 자신은 무시
            if (hit.collider.gameObject == this.gameObject) continue;

            // 지거를 클릭했다면? -> 술 따르기
            if (hit.collider.TryGetComponent(out Jigger jigger))
            {
                // 지거에 술을 채우는 시도
                bool success = jigger.FillFromBottle(alcoholData);

                if (success)
                {
                    Debug.Log($"{alcoholData.alcoholName}을(를) 지거에 따랐습니다.");
                    ReturnToOriginalPosition(); // 성공하면 술병은 제자리로
                    return;
                }
            }
        }

        // 아무것도 클릭하지 않았거나 유효하지 않은 클릭이면 -> 술병 제자리로
        ReturnToOriginalPosition();
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