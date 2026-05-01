using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Alkuul.Audio
{
    /// <summary>
    /// 버튼에 부착하면 클릭 시 자동으로 사운드 재생.
    ///
    /// 사용법:
    /// 1. 클릭음 넣고 싶은 Button GameObject에 이 컴포넌트 추가
    /// 2. Inspector에서 Sound Id 선택 (예: SFX_TitleClick)
    /// 3. 끝.
    ///
    /// Button 컴포넌트가 있으면 onClick 이벤트로 재생 (Button이 비활성화면 안 울림 = 자연스러움).
    /// Button이 없으면 IPointerClickHandler 폴백.
    ///
    /// 한 번에 여러 버튼에 일괄 적용하고 싶으면:
    /// 버튼 여러 개 선택 후 Inspector에서 Add Component → UI Click Sound 추가하면
    /// 선택한 모든 버튼에 동시 추가됨.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIClickSound : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private SoundId soundId = SoundId.SFX_GameClick;

        [Tooltip("이 버튼이 disabled 상태일 때도 클릭음을 낼지 (보통 false 권장)")]
        [SerializeField] private bool playEvenWhenDisabled = false;

        [Tooltip("Button.onClick에 자동 등록할지. 끄면 IPointerClickHandler로만 동작.")]
        [SerializeField] private bool hookButtonOnClick = true;

        private Button _button;
        private bool _hookedToButton;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (hookButtonOnClick && _button != null && !_hookedToButton)
            {
                _button.onClick.AddListener(PlaySound);
                _hookedToButton = true;
            }
        }

        private void OnDisable()
        {
            if (_hookedToButton && _button != null)
            {
                _button.onClick.RemoveListener(PlaySound);
                _hookedToButton = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Button이 있으면 onClick으로 재생되니 중복 방지 위해 여기선 스킵
            if (_button != null && hookButtonOnClick) return;

            PlaySound();
        }

        public void PlaySound()
        {
            if (!playEvenWhenDisabled && _button != null && !_button.IsInteractable())
                return;

            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[UIClickSound] AudioManager.Instance is null.");
                return;
            }

            AudioManager.Instance.Play(soundId);
        }

        /// <summary>외부에서 사운드 ID 변경하고 싶을 때.</summary>
        public void SetSoundId(SoundId id)
        {
            soundId = id;
        }
    }
}
