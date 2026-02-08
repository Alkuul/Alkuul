using System.Text;
using TMPro;
using UnityEngine;
using Alkuul.Domain;
using Alkuul.UI;

namespace Alkuul.DebugTools
{
    /// <summary>
    /// 디버그/테스트용 UI:
    /// - 현재 Flow 상태와 주문 대사를 텍스트로 보여줌
    /// - 버튼으로 StartDay / ReceiveCustomer / ReceiveOrder / StartBrewing / Settlement 호출 가능
    /// </summary>
    public class OrderDebugUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InGameFlowController flow;

        [Header("TMP Output")]
        [SerializeField] private TMP_Text stateText;   // 예: DayPrepared, Awaiting...
        [SerializeField] private TMP_Text dialogueText; // 예: 현재 대사/프롬프트
        [SerializeField] private TMP_Text extraText;   // 선택: 고객/주문 번호 등

        [Header("Update")]
        [SerializeField] private bool autoRefresh = true;
        [SerializeField] private float refreshInterval = 0.2f;

        private float _t;

        private void Awake()
        {
            if (flow == null) flow = FindObjectOfType<InGameFlowController>(true);
            RefreshNow();
        }

        private void Update()
        {
            if (!autoRefresh) return;

            _t += Time.unscaledDeltaTime;
            if (_t >= refreshInterval)
            {
                _t = 0f;
                RefreshNow();
            }
        }

        public void RefreshNow()
        {
            if (flow == null) flow = FindObjectOfType<InGameFlowController>(true);
            if (flow == null)
            {
                if (stateText) stateText.text = "[Debug] Flow not found";
                if (dialogueText) dialogueText.text = "";
                if (extraText) extraText.text = "";
                return;
            }

            // 1) 상태 표시
            if (stateText != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[Flow State]");
                sb.AppendLine($"DayPrepared: {flow.DayPrepared}");
                sb.AppendLine($"AwaitReceiveCustomer: {flow.AwaitingReceiveCustomer}");
                sb.AppendLine($"AwaitReceiveOrder: {flow.AwaitingReceiveOrder}");
                sb.AppendLine($"AwaitRename: {flow.AwaitingRename}");
                sb.AppendLine($"AwaitSettlement: {flow.AwaitingSettlement}");
                stateText.text = sb.ToString().TrimEnd();
            }

            // 2) 현재 대사/프롬프트 표시
            if (dialogueText != null || extraText != null)
            {
                if (flow.TryGetCurrentOrderDialogue(out var profile, out var idx, out var cnt, out var line, out var showMeta))
                {
                    if (dialogueText != null)
                        dialogueText.text = line ?? "";

                    if (extraText != null)
                    {
                        if (cnt <= 0)
                        {
                            extraText.text = "";
                        }
                        else
                        {
                            string cname = string.IsNullOrWhiteSpace(profile.displayName) ? profile.id : profile.displayName;
                            extraText.text = $"{cname} | 주문 {idx}/{cnt}";
                        }
                    }
                }
                else
                {
                    if (dialogueText != null) dialogueText.text = "(dialogue 없음)";
                    if (extraText != null) extraText.text = "";
                }
            }
        }

        // -------------------------
        // 버튼용 함수들 (필요한 것만 연결)
        // -------------------------
        public void OnClick_StartDay()
        {
            if (flow == null) flow = FindObjectOfType<InGameFlowController>(true);
            flow?.StartDay();
            RefreshNow();
        }

        public void OnClick_ReceiveCustomer()
        {
            if (flow == null) flow = FindObjectOfType<InGameFlowController>(true);
            flow?.ReceiveCustomer();
            RefreshNow();
        }

        public void OnClick_ReceiveOrder()
        {
            if (flow == null) flow = FindObjectOfType<InGameFlowController>(true);
            flow?.OnClickReceiveOrder();
            RefreshNow();
        }

        public void OnClick_StartBrewing()
        {
            if (flow == null) flow = FindObjectOfType<InGameFlowController>(true);
            flow?.OnClickStartBrewing();
            RefreshNow();
        }

        public void OnClick_Settlement()
        {
            if (flow == null) flow = FindObjectOfType<InGameFlowController>(true);
            flow?.OnClickSettlement();
            RefreshNow();
        }
    }
}
