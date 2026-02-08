using UnityEngine;
using UnityEngine.UI;
using Alkuul.Domain;
using UnityEngine.EventSystems;

public class UIJiggerController : MonoBehaviour, IPointerClickHandler
{
    [Header("Refs")]
    [SerializeField] private Image icon;
    [SerializeField] private Text mlText; // TMP면 TMP_Text로 교체
    [SerializeField] private UIIngredientData jiggerData;

    [Header("ML Presets")]
    [SerializeField] private float[] presets = new float[] { 15f, 30f, 45f, 60f };
    [SerializeField] private int presetIndex = 1; // 기본 30ml

    private void Awake()
    {
        if (jiggerData == null) jiggerData = GetComponent<UIIngredientData>();
        ApplyMlToData();
        RefreshUI();
    }

    public void SetIngredient(IngredientSO ing)
    {
        if (jiggerData == null) return;

        jiggerData.ingredient = ing;
        // 아이콘 갱신
        if (icon != null) icon.sprite = ing != null ? ing.icon : null;

        RefreshUI();
    }

    public void Clear()
    {
        SetIngredient(null);
    }

    public void ToggleMl()
    {
        if (jiggerData == null) return;

        // 예: 15ml ↔ 30ml 토글
        jiggerData.ml = Mathf.Approximately(jiggerData.ml, 15f) ? 30f : 15f;

        RefreshUI();
    }

    public void StepUp()
    {
        presetIndex = Mathf.Clamp(presetIndex + 1, 0, presets.Length - 1);
        ApplyMlToData();
        RefreshUI();
    }

    public void StepDown()
    {
        presetIndex = Mathf.Clamp(presetIndex - 1, 0, presets.Length - 1);
        ApplyMlToData();
        RefreshUI();
    }

    private void ApplyMlToData()
    {
        if (jiggerData == null) return;
        jiggerData.ml = presets != null && presets.Length > 0 ? presets[presetIndex] : 30f;
    }

    private void RefreshUI()
    {
        if (mlText != null)
            mlText.text = $"{(jiggerData != null ? jiggerData.ml : 0f)} ml";

        // 재료 비어있으면 흐리게 표시(선택)
        if (icon != null)
            icon.enabled = (jiggerData != null && jiggerData.ingredient != null);
    }

    [SerializeField] private bool clickToToggleMl = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!clickToToggleMl) return;
        ToggleMl();
    }
}
