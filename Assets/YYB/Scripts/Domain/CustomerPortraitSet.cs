using UnityEngine;
using Alkuul.Domain;

[CreateAssetMenu(menuName = "Alkuul/Customer/Portrait Set")]
public class CustomerPortraitSet : ScriptableObject
{
    [Header("Stage Sprites (UI Image)")]
    public Sprite sober;
    public Sprite tipsy;
    public Sprite drunk;
    public Sprite wasted;

    [Header("Optional Anim Controllers")]
    public RuntimeAnimatorController wastedLoopController;   // 만취 루프
    public RuntimeAnimatorController dragEvictController;    // 드래그 중(내쫓기)
    public RuntimeAnimatorController dragSleepController;    // 드래그 중(재우기) - 필요하면

    [Header("Optional Drag Sprites (if no animator)")]
    public Sprite dragEvictSprite;
    public Sprite dragSleepSprite;

    public Sprite GetStageSprite(IntoxStage stage)
    {
        return stage switch
        {
            IntoxStage.Sober => sober,
            IntoxStage.Tipsy => tipsy,
            IntoxStage.Drunk => drunk,
            IntoxStage.Wasted => wasted,
            _ => sober
        };
    }
}
