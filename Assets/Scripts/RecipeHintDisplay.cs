using TMPro;
using UnityEngine;

namespace TheLastMooncake.Recipe
{
    public sealed class RecipeHintDisplay : MonoBehaviour
    {
        [SerializeField] private RecipeFeedback feedback;
        [SerializeField] private RecipeSelection selection;
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI message;

        private void OnEnable()
        {
            if (feedback != null)
            {
                feedback.RecipeCorrect += Hide;
                feedback.HintRequested += ShowHint;
                feedback.ExplicitCorrectionRequested += ShowCorrection;
            }

            if (selection != null)
            {
                selection.RecipeChanged += HandleRecipeChanged;
            }

            Hide();
        }

        private void OnDisable()
        {
            if (feedback != null)
            {
                feedback.RecipeCorrect -= Hide;
                feedback.HintRequested -= ShowHint;
                feedback.ExplicitCorrectionRequested -= ShowCorrection;
            }

            if (selection != null)
            {
                selection.RecipeChanged -= HandleRecipeChanged;
            }
        }

        private void ShowHint(RecipeCategory category)
        {
            Show(category switch
            {
                RecipeCategory.Filling => "Hint: Think about which seeds were ground into the filling.",
                RecipeCategory.Centre => "Hint: Remember the little moon placed in the middle.",
                RecipeCategory.Sweetness => "Hint: The filling should not taste like syrup.",
                RecipeCategory.Finish => "Hint: Recall the floral scent while the cakes cooled.",
                _ => "One choice conflicts with the memory."
            });
        }

        private void ShowCorrection(RecipeCategory category)
        {
            Show(category switch
            {
                RecipeCategory.Filling => "Try lotus paste for the filling.",
                RecipeCategory.Centre => "Place one salted egg yolk in the centre.",
                RecipeCategory.Sweetness => "Choose less sugar.",
                RecipeCategory.Finish => "Finish with osmanthus.",
                _ => "Review the remembered recipe and try again."
            });
        }

        private void Show(string text)
        {
            if (message != null)
            {
                message.text = text;
            }

            panel?.SetActive(true);
            feedbackPanel?.SetActive(true);
        }

        private void HandleRecipeChanged(RecipeCategory category, RecipeChoice choice)
        {
            if (choice != RecipeChoice.None)
            {
                Hide();
            }
        }

        public void Hide()
        {
            panel?.SetActive(false);
            feedbackPanel?.SetActive(false);
        }
    }
}
