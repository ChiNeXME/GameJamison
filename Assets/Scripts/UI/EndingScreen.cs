using TheLastMooncake.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastMooncake.UI
{
    public sealed class EndingScreen : MonoBehaviour
    {
        [SerializeField] private GameFlowController flow = null;
        [SerializeField] private GameObject panel = null;
        [SerializeField] private string titleSceneName = "MainMenu";

        private void OnEnable()
        {
            if (flow != null)
            {
                flow.StateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (flow != null)
            {
                flow.StateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            panel?.SetActive(state == GameState.Ending);
        }

        public void PlayAgain()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToTitle()
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }
}
