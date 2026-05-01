namespace Alkuul.Audio
{
    /// <summary>
    /// 게임에서 사용하는 모든 사운드의 ID.
    /// 코드에서는 항상 이 enum으로 사운드를 참조한다.
    /// 실제 AudioClip 매핑은 SoundLibrary 에셋에서 한다.
    /// </summary>
    public enum SoundId
    {
        None = 0,

        // ===== BGM =====
        BGM_Intro = 100,        // 게임 시작, 튜토리얼, 1일차 마지막 손님 퇴장 후
        BGM_Customer1 = 101,    // 1번째 손님 입장
        BGM_Customer2 = 102,    // 2번째 손님 입장
        BGM_Customer3 = 103,    // 3번째 손님 입장

        // ===== UI Click =====
        SFX_TitleClick = 200,   // 타이틀 화면 버튼
        SFX_TextClick = 201,    // 텍스트/대화 넘기기
        SFX_GameClick = 202,    // 인게임 진행 버튼

        // ===== Brewing =====
        SFX_Jigger = 300,       // 술을 지거에 담을 때
        SFX_Pour = 301,         // 술을 따를 때 (지거 → 믹싱글래스)
        SFX_Ice = 302,          // 얼음 추가
        SFX_Garnish = 303,      // 가니쉬 추가

        // ===== Technique (미니게임 루프, 추후 추가) =====
        SFX_Shaking = 400,
        SFX_Stirring = 401,
        SFX_Rolling = 402,
        SFX_Blending = 403,
        SFX_Carbonation = 404,
        SFX_Smoking = 405,
    }
}
