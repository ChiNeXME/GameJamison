using TheLastMooncake.Flow;
using UnityEngine;
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

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (IsPaused)
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
