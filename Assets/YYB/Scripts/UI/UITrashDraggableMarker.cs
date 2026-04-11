using UnityEngine;

public class UITrashDraggableMarker : MonoBehaviour
{
    public enum TrashKind
    {
        None = 0,
        MixingGlass,
        FinishPreviewGlass
    }

    [SerializeField] private TrashKind kind = TrashKind.None;

    public TrashKind Kind => kind;
}