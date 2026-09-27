using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastMooncake.Recipe
{
    public sealed class RecipeFeedback : MonoBehaviour
    {
        [SerializeField] private RecipeValidator validator = null;
        [SerializeField] private RecipeSelection selection = null;
        [SerializeField] private UnityEvent onCorrectRecipe = new();
        [SerializeField] private UnityEvent onWrongRecipe = new();
        [SerializeField] private UnityEvent<RecipeCategory> onHintRequested = new();
        [SerializeField] private UnityEvent<RecipeCategory> onExplicitCorrectionRequested = new();
        [SerializeField] private AudioClip correctSound = null;
        [SerializeField] private AudioClip wrongSound = null;
        [Tooltip("Loudness of the correct/wrong sounds; above 1 boosts them past the clip's own level.")]
        [SerializeField, Range(0f, 3f)] private float soundVolume = 2f;

        public int FailedAttempts { get; private set; }
        public UnityEvent OnCorrectRecipe => onCorrectRecipe;
        public UnityEvent OnWrongRecipe => onWrongRecipe;
        public UnityEvent<RecipeCategory> OnHintRequested => onHintRequested;
        public UnityEvent<RecipeCategory> OnExplicitCorrectionRequested => onExplicitCorrectionRequested;
        public event Action RecipeCorrect;
        public event Action RecipeWrong;
        public event Action<RecipeCategory> HintRequested;
        public event Action<RecipeCategory> ExplicitCorrectionRequested;

        private void OnEnable()
        {
            if (validator != null)
            {
                validator.Validated += HandleValidation;
            }
        }

        private void OnDisable()
        {
            if (validator != null)
            {
                validator.Validated -= HandleValidation;
            }
        }

        public void ResetAttempts()
        {
            FailedAttempts = 0;
        }

        private void HandleValidation(RecipeValidationResult result)
        {
            if (result.IsCorrect)
            {
                FailedAttempts = 0;
                PlaySound(correctSound, soundVolume);
                onCorrectRecipe?.Invoke();
                RecipeCorrect?.Invoke();
                return;
            }

            FailedAttempts++;
            PlaySound(wrongSound, soundVolume);
            onWrongRecipe?.Invoke();
            RecipeWrong?.Invoke();

            RecipeCategory relevantCategory = result.IncorrectCategories[0];
            if (FailedAttempts == 1)
            {
                onHintRequested?.Invoke(relevantCategory);
                HintRequested?.Invoke(relevantCategory);
            }
            else
            {
                onExplicitCorrectionRequested?.Invoke(relevantCategory);
                ExplicitCorrectionRequested?.Invoke(relevantCategory);
            }

            selection?.ClearAll();
        }

        // Plays through the shared SFX player (SFX volume); it is missing when CafeTime is played directly.
        private static void PlaySound(AudioClip clip, float volume)
        {
            AudioSource source = GlobalSFXPlayer.instance != null ? GlobalSFXPlayer.instance.GetComponent<AudioSource>() : null;
            if (clip != null && source != null)
            {
                source.PlayOneShot(clip, volume);
            }
        }
    }
}
