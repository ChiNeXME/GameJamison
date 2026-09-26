using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastMooncake.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "CafeTime";
        [SerializeField] private GameObject SettingsPanel;
        public void Play()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Settings()
        {
            SettingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            SettingsPanel.SetActive(false);
        }
    }
}
