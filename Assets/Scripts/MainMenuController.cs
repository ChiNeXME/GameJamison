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
            SceneFader.LoadScene(gameSceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Settings live on the persistent SettingsCanvas; this scene's own copy is destroyed
        // when the menu is loaded again, so go through the surviving instance.
        public void Settings()
        {
            if (SettingsScript.instance != null)
                SettingsScript.instance.Open();
            else if (SettingsPanel != null)
                SettingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (SettingsScript.instance != null)
                SettingsScript.instance.Close();
            else if (SettingsPanel != null)
                SettingsPanel.SetActive(false);
        }
    }
}
