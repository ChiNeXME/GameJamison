using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastMooncake.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "CafeTime";

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
    }
}
