using System.Text;
using Alkuul.Domain;
using TMPro;
using UnityEngine;

public class UIMixContentsView : MonoBehaviour
{
    [SerializeField] private BrewingPanelBridge bridge;
    [SerializeField] private TMP_Text text;

    [Header("Emotion Display")]
    [SerializeField] private int emotionTopK = 3;
    [SerializeField] private float emotionMinPct = 3f;

    private int lastCount = -1;
    private bool lastIce = false;

    private void Awake()
    {
        if (bridge == null) bridge = FindObjectOfType<BrewingPanelBridge>(true);
        if (text == null) text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (bridge == null || text == null) return;

        int count = bridge.CurrentPortionCount;
        bool ice = bridge.UsesIce;

        // 변화 없으면 갱신 안 함(Compute 호출 최소화)
        if (count == lastCount && ice == lastIce) return;
        Debug.Log($"[MixUI] bridgeID={bridge.GetInstanceID()} count={bridge.CurrentPortionCount}");
        lastCount = count;
        lastIce = ice;

        var d = bridge.PreviewDrink();
        text.text = BuildString(d, ice);
    }

    private string BuildString(Drink d, bool ice)
    {
        var sb = new StringBuilder();

        sb.AppendLine("믹싱글라스");
        sb.AppendLine($"총량: {d.totalMl:0.#} ml" + (ice ? " (얼음 포함)" : ""));
        sb.AppendLine($"도수: {d.finalABV:0.#}%");

        sb.AppendLine("감정(프리뷰):");
        sb.AppendLine(EmotionFormat.ToPercentLines(d.emotions, emotionTopK, emotionMinPct));

        return sb.ToString().TrimEnd();
    }
}
