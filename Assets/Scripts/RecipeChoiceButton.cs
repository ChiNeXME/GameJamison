using UnityEngine;
using UnityEngine.UI;

namespace TheLastMooncake.Recipe
{
    /// <summary>
    /// Connects a Unity UI button to a non-physical recipe decision, such as
    /// leaving the centre empty or choosing the amount of sugar.
    /// </summary>
    public sealed class RecipeChoiceButton : MonoBehaviour
    {
        [SerializeField] private RecipeSelection selection = null;
        [SerializeField] private RecipeCategory category = RecipeCategory.Centre;
        [SerializeField] private RecipeChoice choice = RecipeChoice.NoYolk;
        [SerializeField] private Button button = null;
        [SerializeField] private GameObject selectedIndicator = null;

        private void OnEnable()
        {
            if (selection != null)
            {
                selection.RecipeChanged += HandleRecipeChanged;
                RefreshSelectedState();
            }
        }

        private void OnDisable()
        {
            if (selection != null)
            {
                selection.RecipeChanged -= HandleRecipeChanged;
            }
        }

        public void Select()
        {
            if (selection == null)
            {
                Debug.LogError("RecipeChoiceButton needs a RecipeSelection reference.", this);
                return;
            }

            selection.SelectChoice(category, choice);
        }

        private void HandleRecipeChanged(RecipeCategory changedCategory, RecipeChoice selectedChoice)
        {
            if (changedCategory == category)
            {
                SetSelected(selectedChoice == choice);
            }
        }

        private void RefreshSelectedState()
        {
            bool selected = selection.TryGetChoice(category, out RecipeChoice selectedChoice) &&
                            selectedChoice == choice;
            SetSelected(selected);
        }

        private void SetSelected(bool selected)
        {
            if (button != null)
            {
                button.interactable = !selected;
            }

            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(selected);
            }
        }

        private void OnValidate()
        {
            if (!RecipeRules.IsChoiceForCategory(category, choice))
            {
                Debug.LogWarning($"{choice} is not a valid choice for {category}.", this);
            }
        }
    }
}
