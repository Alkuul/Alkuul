using System.Collections.Generic;
using UnityEngine;

public class MixingGlass : MonoBehaviour
{
    private float currentTotalVolume = 0f;

    // 변경된 7가지 감정 누적 변수
    private float totalHappiness = 0;
    private float totalSadness = 0;
    private float totalAnger = 0;
    private float totalFear = 0;         // 공포
    private float totalDisgust = 0;      // 혐오
    private float totalSurprise = 0;     // 놀람
    private float totalIndifference = 0; // 무감정

    public void AddLiquid(AlcoholData data, float volume)
    {
        currentTotalVolume += volume;

        // 각 감정별 (용량 * 비율) 누적
        totalHappiness += volume * data.happiness;
        totalSadness += volume * data.sadness;
        totalAnger += volume * data.anger;

        // 새로 추가된 감정들 계산
        totalFear += volume * data.fear;
        totalDisgust += volume * data.disgust;
        totalSurprise += volume * data.surprise;
        totalIndifference += volume * data.indifference;

        Debug.Log($"믹싱글라스에 {data.alcoholName} {volume}ml 추가됨.");
        CalculateCurrentRatio();
    }

    public void CalculateCurrentRatio()
    {
        if (currentTotalVolume == 0) return;

        // 결과 출력 (소수점 첫째자리까지 표시 :F1)
        Debug.Log($"[현재 칵테일 성분 비율]\n" +
                  $"행복: {totalHappiness / currentTotalVolume:F1}%\n" +
                  $"슬픔: {totalSadness / currentTotalVolume:F1}%\n" +
                  $"분노: {totalAnger / currentTotalVolume:F1}%\n" +
                  $"공포: {totalFear / currentTotalVolume:F1}%\n" +
                  $"혐오: {totalDisgust / currentTotalVolume:F1}%\n" +
                  $"놀람: {totalSurprise / currentTotalVolume:F1}%\n" +
                  $"무감정: {totalIndifference / currentTotalVolume:F1}%");
    }
}