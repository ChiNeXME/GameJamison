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

    public static class RecipeRules
    {
        public static bool IsChoiceForCategory(RecipeCategory category, RecipeChoice choice)
        {
            return category switch
            {
                RecipeCategory.Filling => choice is RecipeChoice.Lotus or RecipeChoice.RedBean,
                RecipeCategory.Centre => choice is RecipeChoice.SaltedYolk or RecipeChoice.NoYolk,
                RecipeCategory.Sweetness => choice is RecipeChoice.LowSweetness or RecipeChoice.RegularSweetness,
                RecipeCategory.Finish => choice is RecipeChoice.Osmanthus or RecipeChoice.Sesame,
                _ => false
            };
        }
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
