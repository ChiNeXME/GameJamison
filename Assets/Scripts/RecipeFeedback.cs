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
                onCorrectRecipe?.Invoke();
                RecipeCorrect?.Invoke();
                return;
            }

            FailedAttempts++;
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
    }
}
