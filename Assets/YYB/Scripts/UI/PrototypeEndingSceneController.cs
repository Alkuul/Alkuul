using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alkuul.UI
{
    public class PrototypeEndingSceneController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text averageSatisfactionText;
        [SerializeField] private TMP_Text summaryText;

        [Header("Scene Names")]
        [SerializeField] private string titleSceneName = "TitleScene";

        private void Start()
        {
            if (averageSatisfactionText != null)
                averageSatisfactionText.text = $"평균 만족도 {PrototypeEndingContext.AverageSatisfaction:F1}%";

            if (summaryText != null)
                summaryText.text =
                    $"총 손님 수 {PrototypeEndingContext.TotalCustomers}명\n" +
                    $"{PrototypeEndingContext.FinalDay}일차 정산 완료";
        }

        public void OnClickGoTitle()
        {
            PrototypeEndingContext.Clear();
            SceneManager.LoadScene(titleSceneName);
        }
    }
}