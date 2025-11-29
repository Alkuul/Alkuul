using UnityEngine;
using Alkuul.Domain;
using Alkuul.UI;

public class AddPortionButton : MonoBehaviour
{
    [SerializeField] private BrewingUI target;     // Canvas의 BrewingUIController 드래그
    [SerializeField] private IngredientSO ingredient; // 버튼용 재료 에셋
    [SerializeField] private float ml = 30f;          // 이 버튼이 넣을 양

    public void Add()
    {
        if (target == null || ingredient == null) { Debug.LogWarning("AddPortionButton: ref missing"); return; }
        target.OnAdd(ingredient, ml);
    }
}
