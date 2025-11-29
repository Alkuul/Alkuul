using System.Collections.Generic;
using UnityEngine;

public class MixingGlass : MonoBehaviour
{
    // 현재 담긴 총 용량
    private float currentTotalVolume = 0f;

    // 각 감정별 축적된 양 (용량 * 비율)
    private float totalAnger = 0;
    private float totalSadness = 0;
    private float totalHappiness = 0;
    private float totalPrickly = 0;
    private float totalTimid = 0;

    public void AddLiquid(AlcoholData data, float volume)
    {
        currentTotalVolume += volume;

        // (들어온 용량 * 해당 감정의 비율)을 누적
        totalAnger += volume * data.anger;
        totalSadness += volume * data.sadness;
        totalHappiness += volume * data.happiness;
        totalPrickly += volume * data.prickly;
        totalTimid += volume * data.timid;

        Debug.Log($"믹싱글라스에 {data.alcoholName} {volume}ml 추가됨. 총량: {currentTotalVolume}ml");
        CalculateCurrentRatio();
    }

    // 현재 비율 계산 (디버그용)
    public void CalculateCurrentRatio()
    {
        if (currentTotalVolume == 0) return;

        Debug.Log($"[현재 칵테일 비율] " +
                  $"분노: {totalAnger / currentTotalVolume:F1}% " +
                  $"슬픔: {totalSadness / currentTotalVolume:F1}% " +
                  $"행복: {totalHappiness / currentTotalVolume:F1}% " +
                  $"까칠: {totalPrickly / currentTotalVolume:F1}% " +
                  $"소심: {totalTimid / currentTotalVolume:F1}%");
    }
}