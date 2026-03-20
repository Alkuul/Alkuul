using Alkuul.Domain;
using UnityEngine;

public class TechniqueButtonBinder : MonoBehaviour
{
    [SerializeField] private TechniqueSO techniqueData;
    [SerializeField] private TechniqueInteractionController controller;

    public void OnClickTechnique()
    {
        if (controller == null)
        {
            Debug.LogWarning("[TechniqueButtonBinder] Controller is null.");
            return;
        }

        if (techniqueData == null)
        {
            Debug.LogWarning("[TechniqueButtonBinder] Technique data is null.");
            return;
        }

        controller.StartTechniqueInteraction(techniqueData);
    }
}
