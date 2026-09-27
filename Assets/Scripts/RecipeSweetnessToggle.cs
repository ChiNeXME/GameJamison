using UnityEngine;
using UnityEngine.UI;

namespace TheLastMooncake.Recipe
{
    /// <summary>
    /// A single button that toggles the Sweetness choice between "regular" and
    /// "less sugar", swapping its icon to match whichever is currently selected.
    /// </summary>
    public sealed class RecipeSweetnessToggle : MonoBehaviour
    {
        [SerializeField] private RecipeSelection selection = null;
        [SerializeField] private Image icon = null;
        [SerializeField] private Sprite regularSprite = null;
        [SerializeField] private Sprite lessSprite = null;

        private void OnEnable()
        {
            if (selection != null)
            {
                selection.RecipeChanged += HandleRecipeChanged;
            }

            RefreshIcon();
        }

        private void OnDisable()
        {
            if (selection != null)
            {
                selection.RecipeChanged -= HandleRecipeChanged;
            }
        }

        public void Toggle()
        {
            if (selection == null)
            {
                Debug.LogError("RecipeSweetnessToggle needs a RecipeSelection reference.", this);
                return;
            }

            selection.TryGetChoice(RecipeCategory.Sweetness, out RecipeChoice current);

            RecipeChoice next = current == RecipeChoice.LowSweetness
                ? RecipeChoice.RegularSweetness
                : RecipeChoice.LowSweetness;

            selection.SelectChoice(RecipeCategory.Sweetness, next);
        }

        private void HandleRecipeChanged(RecipeCategory category, RecipeChoice choice)
        {
            if (category == RecipeCategory.Sweetness)
            {
                RefreshIcon();
            }
        }

        private void RefreshIcon()
        {
            if (icon == null || selection == null)
            {
                return;
            }

            selection.TryGetChoice(RecipeCategory.Sweetness, out RecipeChoice choice);
            icon.sprite = choice == RecipeChoice.LowSweetness ? lessSprite : regularSprite;
        }
    }
}