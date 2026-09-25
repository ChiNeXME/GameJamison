using System;
using System.Collections.Generic;
using TheLastMooncake.Customers;
using UnityEngine;

namespace TheLastMooncake.Recipe
{
    public sealed class RecipeValidator : MonoBehaviour
    {
        [SerializeField] private RecipeSelection selection = null;

        private CustomerCase activeCase;

        public event Action<RecipeValidationResult> Validated;

        /// <summary>
        /// Raised instead of Validated when the recipe still has empty categories,
        /// so an unfinished recipe does not count as a failed attempt.
        /// </summary>
        public event Action<IReadOnlyList<RecipeCategory>> SubmitBlocked;

        public void SetCase(CustomerCase customerCase)
        {
            activeCase = customerCase;
        }

        public RecipeValidationResult ValidateCurrentRecipe()
        {
            List<RecipeCategory> incorrect = new();

            foreach (RecipeCategory category in RecipeRules.AllCategories)
            {
                RecipeChoice expected = activeCase != null ? activeCase.GetAnswer(category) : RecipeChoice.None;
                Check(category, expected, incorrect);
            }

            return new RecipeValidationResult(incorrect);
        }

        public void Submit()
        {
            if (activeCase == null)
            {
                Debug.LogError("RecipeValidator has no active customer case.", this);
                return;
            }

            List<RecipeCategory> missing = GetMissingCategories();
            if (missing.Count > 0)
            {
                SubmitBlocked?.Invoke(missing);
                return;
            }

            RecipeValidationResult result = ValidateCurrentRecipe();
            Validated?.Invoke(result);
        }

        private List<RecipeCategory> GetMissingCategories()
        {
            List<RecipeCategory> missing = new();
            foreach (RecipeCategory category in RecipeRules.AllCategories)
            {
                if (selection == null ||
                    !selection.TryGetChoice(category, out RecipeChoice choice) ||
                    choice == RecipeChoice.None)
                {
                    missing.Add(category);
                }
            }

            return missing;
        }

        private void Check(
            RecipeCategory category,
            RecipeChoice expected,
            ICollection<RecipeCategory> incorrect)
        {
            if (selection == null ||
                !selection.TryGetChoice(category, out RecipeChoice actual) ||
                actual != expected)
            {
                incorrect.Add(category);
            }
        }
    }
}
