using UnityEngine;
using UnityEngine.EventSystems;

public class JiggerClickToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UIJiggerController jigger;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (jigger == null) jigger = GetComponent<UIJiggerController>();
        jigger?.ToggleMl();
    }
}
