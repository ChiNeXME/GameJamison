using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TheLastMooncake.Recipe
{
    /// <summary>
    /// Shows what is currently chosen for each category, and which categories are
    /// still empty when the player tries to submit an unfinished recipe.
    /// </summary>
    public sealed class RecipeStatusDisplay : MonoBehaviour
    {
        [SerializeField] private RecipeSelection selection = null;
        [SerializeField] private RecipeValidator validator = null;
        [SerializeField] private TextMeshProUGUI label = null;
        [SerializeField, Min(0f)] private float warningDuration = 2.5f;

        private string warning;
        private float warningUntil;

        private void OnEnable()
        {
            if (selection != null)
            {
                selection.RecipeChanged += HandleRecipeChanged;
            }

            if (validator != null)
            {
                validator.SubmitBlocked += ShowMissing;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (selection != null)
            {
                selection.RecipeChanged -= HandleRecipeChanged;
            }

            if (validator != null)
            {
                validator.SubmitBlocked -= ShowMissing;
            }
        }

        private void Update()
        {
            if (warning != null && Time.unscaledTime >= warningUntil)
            {
                warning = null;
                Refresh();
            }
        }

        private void HandleRecipeChanged(RecipeCategory category, RecipeChoice choice)
        {
            Refresh();
        }

        private void ShowMissing(IReadOnlyList<RecipeCategory> missing)
        {
            warning = "Still to choose: " + string.Join(", ", missing.Select(category => category.ToString()));
            warningUntil = Time.unscaledTime + warningDuration;
            Refresh();
        }

        private void Refresh()
        {
            if (label == null)
            {
                return;
            }

            IEnumerable<string> parts = RecipeRules.AllCategories.Select(category =>
            {
                RecipeChoice choice = RecipeChoice.None;
                selection?.TryGetChoice(category, out choice);
                return $"{category}: {RecipeRules.GetDisplayName(choice)}";
            });

            string text = string.Join("   |   ", parts);
            if (warning != null)
            {
                text += $"\n<color=#FF9A7A>{warning}</color>";
            }

            label.text = text;
        }
    }
}
