using UnityEngine;
using UnityEngine.EventSystems;

public class JiggerFillDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private UIJiggerController jigger;
    [SerializeField] private BrewingTutorialController tutorial;

    private void Awake()
    {
        if (tutorial == null)
            tutorial = FindObjectOfType<BrewingTutorialController>(true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var go = eventData.pointerDrag;
        if (go == null) return;

        var data = go.GetComponent<UIIngredientData>();
        if (data == null || data.ingredient == null) return;

        if (jigger == null) jigger = GetComponentInParent<UIJiggerController>();
        if (jigger == null) return;

        jigger.SetIngredient(data.ingredient);
        Debug.Log($"[UI] Jigger filled: {data.ingredient.name}");

        // ▼▼▼ 사운드: 지거에 담기 ▼▼▼
        if (Alkuul.Audio.AudioManager.Instance != null)
            Alkuul.Audio.AudioManager.Instance.Play(Alkuul.Audio.SoundId.SFX_Jigger);
        // ▲▲▲

        tutorial?.NotifyPouredToJigger();
    }
}