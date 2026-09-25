using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastMooncake.Recipe
{
    public sealed class RecipeValidator : MonoBehaviour
    {
        [SerializeField] private RecipeSelection selection = null;

        public event Action<RecipeValidationResult> Validated;

        public RecipeValidationResult ValidateCurrentRecipe()
        {
            List<RecipeCategory> incorrect = new();

            Check(RecipeCategory.Filling, RecipeChoice.Lotus, incorrect);
            Check(RecipeCategory.Centre, RecipeChoice.SaltedYolk, incorrect);
            Check(RecipeCategory.Sweetness, RecipeChoice.LowSweetness, incorrect);
            Check(RecipeCategory.Finish, RecipeChoice.Osmanthus, incorrect);

            return new RecipeValidationResult(incorrect);
        }

        public void Submit()
        {
            RecipeValidationResult result = ValidateCurrentRecipe();
            Validated?.Invoke(result);
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
