using TMPro;
using TheLastMooncake.Customers;
using TheLastMooncake.Flow;
using UnityEngine;

namespace TheLastMooncake.Recipe
{
    public sealed class RecipeHintDisplay : MonoBehaviour
    {
        [SerializeField] private RecipeFeedback feedback;
        [SerializeField] private RecipeSelection selection;
        [SerializeField] private CafeSession session;
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
            CaseClue clue = FindClue(category);
            Show(clue != null ? clue.Hint : "One choice conflicts with the memory.");
        }

        private void ShowCorrection(RecipeCategory category)
        {
            CaseClue clue = FindClue(category);
            Show(clue != null ? clue.Correction : "Review the remembered recipe and try again.");
        }

        private CaseClue FindClue(RecipeCategory category)
        {
            CustomerCase customerCase = session != null ? session.CurrentCase : null;
            return customerCase != null ? customerCase.GetClue(category) : null;
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
