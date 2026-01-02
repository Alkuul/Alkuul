using UnityEngine;
using Alkuul.Domain;
using Alkuul.Core;

public class MixingGlass : MonoBehaviour
{
    private float currentTotalVolume = 0f;
    private EmotionVector weightedSum; // (emotion * ml) 누적

    public void AddLiquid(IngredientSO ing, float volume)
    {
        if (ing == null || volume <= 0f) return;

        currentTotalVolume += volume;
        weightedSum = VectorOps.AddWeighted(weightedSum, ing.emotions, volume);

        Debug.Log($"믹싱글라스에 {ing.displayName} {volume}ml 추가됨.");
        CalculateCurrentRatio();
    }

    public void CalculateCurrentRatio()
    {
        if (currentTotalVolume <= 0f) return;

        // (emotion*ml)/totalMl => 현재 비율(0~1 기준이라면 *100해서 출력해도 됨)
        var ratio = weightedSum;
        ratio.joy /= currentTotalVolume;
        ratio.sadness /= currentTotalVolume;
        ratio.anger /= currentTotalVolume;
        ratio.fear /= currentTotalVolume;
        ratio.disgust /= currentTotalVolume;
        ratio.surprise /= currentTotalVolume;
        ratio.neutral /= currentTotalVolume;

        Debug.Log(
            $"[현재 칵테일 성분 비율]\n" +
            $"joy: {ratio.joy * 100f:F1}%\n" +
            $"sadness: {ratio.sadness * 100f:F1}%\n" +
            $"anger: {ratio.anger * 100f:F1}%\n" +
            $"fear: {ratio.fear * 100f:F1}%\n" +
            $"disgust: {ratio.disgust * 100f:F1}%\n" +
            $"surprise: {ratio.surprise * 100f:F1}%\n" +
            $"neutral: {ratio.neutral * 100f:F1}%"
        );
    }

    public void Clear()
    {
        currentTotalVolume = 0f;
        weightedSum = default;
    }
}
