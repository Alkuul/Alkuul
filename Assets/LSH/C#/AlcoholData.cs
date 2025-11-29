using UnityEngine;

[CreateAssetMenu(fileName = "NewAlcohol", menuName = "Cocktail/Alcohol Data")]
public class AlcoholData : ScriptableObject
{
    public string alcoholName; // 술 이름

    [Header("Emotion Percentages (Total usually 100)")]
    [Range(0, 100)] public int anger;     // 분노
    [Range(0, 100)] public int sadness;   // 슬픔
    [Range(0, 100)] public int happiness; // 행복
    [Range(0, 100)] public int prickly;   // 까칠
    [Range(0, 100)] public int timid;     // 소심
}