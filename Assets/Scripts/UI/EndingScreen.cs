using System;
using System.Collections;
using TheLastMooncake.Flow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastMooncake.UI
{
    /// <summary>
    /// Credits screen shown at the end. Its parts (title, each credit, buttons)
    /// fade in one after another; Space or a click shows everything at once.
    /// </summary>
    public sealed class EndingScreen : MonoBehaviour
    {
        [SerializeField] private GameFlowController flow = null;
        [SerializeField] private GameObject panel = null;
        [SerializeField] private string titleSceneName = "MainMenu";

        [Header("Reveal")]
        [SerializeField] private CanvasGroup[] revealInOrder = Array.Empty<CanvasGroup>();
        [SerializeField, Min(0f)] private float firstDelay = 0.6f;
        [SerializeField, Min(0f)] private float stepDelay = 0.7f;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.9f;

        private Coroutine reveal;

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
            bool show = state == GameState.Ending;
            panel?.SetActive(show);
            if (show)
            {
                reveal = StartCoroutine(Reveal());
            }
        }

        private IEnumerator Reveal()
        {
            foreach (CanvasGroup group in revealInOrder)
            {
                SetVisible(group, 0f);
            }

            yield return new WaitForSeconds(firstDelay);
            for (int index = 0; index < revealInOrder.Length; index++)
            {
                StartCoroutine(Fade(revealInOrder[index]));
                yield return WaitOrSkip(stepDelay);
            }

            reveal = null;
        }

        private IEnumerator Fade(CanvasGroup group)
        {
            for (float t = 0f; t < fadeDuration && group.alpha < 1f; t += Time.deltaTime)
            {
                SetVisible(group, Mathf.Max(group.alpha, t / fadeDuration));
                yield return null;
            }

            SetVisible(group, 1f);
        }

        private IEnumerator WaitOrSkip(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    ShowAll();
                    yield break;
                }

                yield return null;
            }
        }

        private void ShowAll()
        {
            if (reveal != null)
            {
                StopCoroutine(reveal);
                reveal = null;
            }

            foreach (CanvasGroup group in revealInOrder)
            {
                SetVisible(group, 1f);
            }
        }

        // The BGM player survives scene loads, so the ending track must be stopped here.
        private static void StopMusic()
        {
            AudioSource music = GlobalBGMPlayer.instance != null ? GlobalBGMPlayer.instance.GetComponent<AudioSource>() : null;
            if (music != null)
            {
                music.Stop();
                music.clip = null;
            }
        }

        private static void SetVisible(CanvasGroup group, float alpha)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = alpha;
            group.interactable = alpha >= 1f;
            group.blocksRaycasts = alpha >= 1f;
        }

        public void PlayAgain()
        {
            StopMusic();
            SceneFader.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToTitle()
        {
            StopMusic();
            SceneFader.LoadScene(titleSceneName);
        }
    }
}
