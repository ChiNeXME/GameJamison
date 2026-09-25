using System;
using System.Collections.Generic;

namespace TheLastMooncake.Recipe
{
    public enum RecipeCategory
    {
        Filling,
        Centre,
        Sweetness,
        Finish
    }

    public enum RecipeChoice
    {
        None,
        Lotus,
        RedBean,
        SaltedYolk,
        NoYolk,
        LowSweetness,
        RegularSweetness,
        Osmanthus,
        Sesame
    }

    public sealed class RecipeValidationResult
    {
        private readonly List<RecipeCategory> incorrectCategories;

        public RecipeValidationResult(List<RecipeCategory> incorrectCategories)
        {
            this.incorrectCategories = incorrectCategories;
        }

        public bool IsCorrect => incorrectCategories.Count == 0;
        public IReadOnlyList<RecipeCategory> IncorrectCategories => incorrectCategories;
    }
}
