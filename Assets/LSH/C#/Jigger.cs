using UnityEngine;
using Alkuul.Domain;
using Alkuul.UI;

public class Jigger : MonoBehaviour
{
    [Header("Settings")]
    public float smallCapacity = 30f;
    public float largeCapacity = 45f;

    [SerializeField] private BrewingPanelBridge bridge;

    private Vector3 originalPosition;
    private bool isHeld = false;
    private bool isFlipped = false;   // false: 30ml, true: 45ml

    private IngredientSO currentContent = null; // 교체: AlcoholData -> IngredientSO
    private bool isFilled = false;

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
        // 우클릭: 용량 전환(들고 있지 않을 때)
        if (Input.GetMouseButtonDown(1) && !isHeld)
        {
            if (IsMouseOver())
                FlipJigger();
        }

        // 좌클릭: 집기 / 붓기
        if (Input.GetMouseButtonDown(0))
        {
            if (!isHeld)
            {
                if (IsMouseOver())
                {
                    if (isFilled)
                    {
                        isHeld = true;
                        spriteRenderer.sortingOrder = 100;
                    }
                    else
                    {
                        Debug.Log("지거가 비어있어서 들 수 없습니다. 먼저 술을 채워주세요.");
                    }
                }
            }
            else
            {
                HandleInteractionWhileHeld();
            }
        }
    }

    // Bottle에서 호출
    public bool FillFromBottle(IngredientSO ing)
    {
        if (ing == null)
        {
            Debug.LogWarning("Jigger: FillFromBottle에 null IngredientSO가 들어왔습니다.");
            return false;
        }

        if (isFilled)
        {
            Debug.Log("지거가 이미 차 있습니다! 버리거나 믹싱글라스에 넣으세요.");
            return false;
        }

        currentContent = ing;
        isFilled = true;

        float currentCap = isFlipped ? largeCapacity : smallCapacity;
        Debug.Log($"지거({currentCap}ml)에 {ing.displayName} 채워짐!");

        // 시각적 피드백
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

            if (hit.collider.TryGetComponent(out MixingGlass glass))
            {
                PourIntoGlass(glass);
                return;
            }
        }

        ReturnToOriginalPosition();
    }

    void PourIntoGlass(MixingGlass glass)
    {
        if (!isFilled || currentContent == null)
        {
            Debug.LogWarning("지거가 비어있습니다.");
            ReturnToOriginalPosition();
            return;
        }

        float capacity = isFlipped ? largeCapacity : smallCapacity;

        var poured = currentContent;
        glass.AddLiquid(poured, capacity);

        if (bridge != null)
            bridge.OnPortionAdded(poured, capacity);

        // 비우기
        currentContent = null;
        isFilled = false;
        spriteRenderer.color = Color.white;

        ReturnToOriginalPosition();
    }

    void FlipJigger()
    {
        if (isFilled)
        {
            Debug.Log("술이 들어있어 뒤집을 수 없습니다.");
            return;
        }

        isFlipped = !isFlipped;
        transform.Rotate(0, 0, 180);
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
