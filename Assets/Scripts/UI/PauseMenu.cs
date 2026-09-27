using TheLastMooncake.Flow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TheLastMooncake.UI
{
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panel = null;
        [SerializeField] private CafeSession session = null;
        [SerializeField] private MemoryNotesPanel notes = null;
        [SerializeField] private string titleSceneName = "MainMenu";

        public bool IsPaused { get; private set; }

        private void Start()
        {
            AddSettingsButton();
        }

        // The settings screen lives in the main menu scene, so the button is made here rather
        // than in CafeTime, and only when that screen exists (not when CafeTime is played directly).
        private void AddSettingsButton()
        {
            Transform titleButton = panel != null ? panel.transform.Find("TitleButton") : null;
            if (SettingsScript.instance == null || titleButton == null)
            {
                return;
            }

            GameObject copy = Instantiate(titleButton.gameObject, titleButton.parent);
            copy.name = "SettingsButton";
            var rect = (RectTransform)copy.transform;
            rect.anchoredPosition += new Vector2(0f, -100f);

            Button button = copy.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(OpenSettings);

            TextMeshProUGUI label = copy.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = "SETTINGS";
            }
        }

        public void OpenSettings()
        {
            SettingsScript.instance?.Open();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (SettingsScript.instance != null && SettingsScript.instance.IsOpen)
            {
                SettingsScript.instance.Close();
            }
            else if (IsPaused)
            {
                Resume();
            }
            else if (notes != null && notes.IsOpen)
            {
                notes.Close();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (IsPaused)
            {
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            panel?.SetActive(true);
            session?.AddInputBlock();
        }

        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            panel?.SetActive(false);
            session?.RemoveInputBlock();
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToTitle()
        {
            SettingsScript.instance?.Close();
            Time.timeScale = 1f;
            SceneManager.LoadScene(titleSceneName);
        }

        private void OnDestroy()
        {
            if (IsPaused)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
