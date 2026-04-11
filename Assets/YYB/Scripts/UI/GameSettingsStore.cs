using UnityEngine;

namespace Alkuul.UI
{
    public static class GameSettingsStore
    {
        private const string KeyMasterVolume = "settings.masterVolume";
        private const string KeyFullscreen = "settings.fullscreen";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            LoadAndApply();
        }

        public static void LoadAndApply()
        {
            ApplyMasterVolume(GetMasterVolume());
            ApplyFullscreen(GetFullscreen());
        }

        public static float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat(KeyMasterVolume, 1f);
        }

        public static void SetMasterVolume(float value)
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KeyMasterVolume, value);
            PlayerPrefs.Save();
            ApplyMasterVolume(value);
        }

        public static bool GetFullscreen()
        {
            return PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;
        }

        public static void SetFullscreen(bool value)
        {
            PlayerPrefs.SetInt(KeyFullscreen, value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyFullscreen(value);
        }

        private static void ApplyMasterVolume(float value)
        {
            AudioListener.volume = Mathf.Clamp01(value);
        }

        private static void ApplyFullscreen(bool value)
        {
            Screen.fullScreen = value;
        }
    }
}
