using UnityEngine;

// UIDraggablePortrait가 Alkuul.UI 네임스페이스면 유지
using Alkuul.UI;

public class OrderCustomerActions : MonoBehaviour
{
    [Header("References (optional)")]
    [SerializeField] private UIDraggablePortrait drag;
    [SerializeField] private TabletController tablet;
    [SerializeField] private PendingInnDecisionSystem pending;

    [Header("Behaviour")]
    [SerializeField] private bool closeTabletAfterDecision = true;   // 결정 끝나면 태블릿 닫기
    [SerializeField] private bool showHomeAfterDecision = true;      // 결정 끝나면 홈 패널로

    private void Awake()
    {
        if (drag == null) drag = FindObjectOfType<UIDraggablePortrait>(true);
        if (tablet == null) tablet = FindObjectOfType<TabletController>(true);
        if (pending == null) pending = FindObjectOfType<PendingInnDecisionSystem>(true);

        if (drag == null)
        {
            Debug.LogError("[OrderCustomerActions] UIDraggablePortrait not found.");
            return;
        }

        // 문 드롭 = 내쫓기
        drag.OnDroppedToDoor += HandleEvict;

        // 침대 드롭 = 재우기
        drag.OnDroppedToBed += HandleSleep;
    }

    private void HandleEvict()
    {
        bool handled = false;

        // 1) PendingInnDecisionSystem 우선 시도(큐가 있어야 동작)
        if (pending != null && pending.HasPending)
        {
            handled = pending.EvictOne();
            Debug.Log($"[OrderCustomerActions] Evict via Pending => {handled}");
        }

        // 2) Pending이 없거나 실패하면 TabletController 방식으로 처리(내부 pendingInn 큐 사용)
        if (!handled && tablet != null)
        {
            tablet.OnClick_Evict();
            handled = true;
            Debug.Log("[OrderCustomerActions] Evict via Tablet");
        }

        if (!handled)
            Debug.LogWarning("[OrderCustomerActions] Evict failed: no pending decision available.");

        //AfterDecisionUI();
    }

    private void HandleSleep()
    {
        bool handled = false;

        // 1) PendingInnDecisionSystem 우선 시도(큐가 있어야 동작)
        if (pending != null && pending.HasPending)
        {
            handled = pending.SleepOne();
            Debug.Log($"[OrderCustomerActions] Sleep via Pending => {handled}");
        }

        // 2) Pending이 없거나 실패하면 TabletController 방식으로 처리
        if (!handled && tablet != null)
        {
            tablet.OnClick_Sleep();
            handled = true;
            Debug.Log("[OrderCustomerActions] Sleep via Tablet");
        }

        if (!handled)
            Debug.LogWarning("[OrderCustomerActions] Sleep failed: no pending decision available.");

        //AfterDecisionUI();
    }

    private void AfterDecisionUI()
    {
        if (tablet == null) return;

        //if (showHomeAfterDecision)
        //    tablet.Show_P1_Home();

        if (closeTabletAfterDecision)
            tablet.SetOpen(false);
    }
}
