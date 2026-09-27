using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TheLastMooncake.Customers;
using TheLastMooncake.Recipe;
using UnityEngine;

namespace TheLastMooncake.Flow
{
    /// <summary>
    /// Runs the story: the opening, then each customer in order (arrival dialogue,
    /// recipe, wrong-recipe reactions, resolution dialogue), then the ending.
    /// </summary>
    public sealed class CafeSession : MonoBehaviour
    {
        [SerializeField] private DialogueManager DM;
        [SerializeField] private CustomerCase[] cases = Array.Empty<CustomerCase>();
        [SerializeField] private GameFlowController flow = null;
        [SerializeField] private RecipeSelection selection = null;
        [SerializeField] private RecipeValidator validator = null;
        [SerializeField] private RecipeFeedback feedback = null;
        [SerializeField] private DragDrop dragDrop = null;
        [SerializeField] private CanvasGroup[] recipeControls = Array.Empty<CanvasGroup>();

        [Header("Story")]
        [SerializeField] private Conversation opening = null;
        [SerializeField] private Conversation ending = null;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI customerLabel = null;
        [SerializeField] private GameObject resolutionPanel = null;
        [SerializeField] private TextMeshProUGUI resolutionText = null;
        [SerializeField] private TextMeshProUGUI continueLabel = null;

        private int caseIndex = -1;
        private int inputBlocks;
        private bool recipeOpen;
        private bool talking;

        public CustomerCase CurrentCase =>
            caseIndex >= 0 && caseIndex < cases.Length ? cases[caseIndex] : null;
        public int CaseCount => cases.Length;
        public int SolvedCount { get; private set; }

        public event Action<CustomerCase> CaseStarted;
        public event Action<CustomerCase> CaseSolved;

        private void OnEnable()
        {
            if (feedback != null)
            {
                feedback.RecipeCorrect += HandleRecipeCorrect;
                feedback.HintRequested += HandleFirstMistake;
                feedback.ExplicitCorrectionRequested += HandleRepeatedMistake;
            }
        }

        private void OnDisable()
        {
            if (feedback != null)
            {
                feedback.RecipeCorrect -= HandleRecipeCorrect;
                feedback.HintRequested -= HandleFirstMistake;
                feedback.ExplicitCorrectionRequested -= HandleRepeatedMistake;
            }
        }

        private void Start()
        {
            if (cases.Length == 0)
            {
                Debug.LogError("CafeSession has no customer cases assigned.", this);
                return;
            }

            StartCoroutine(CaseRoutine(0, opening));
        }

        /// <summary>Continue button on the "recipe correct" panel.</summary>
        public void ContinueToNextCase()
        {
            if (resolutionPanel != null)
            {
                resolutionPanel.SetActive(false);
            }

            StartCoroutine(FinishCaseRoutine());
        }

        /// <summary>
        /// Modal UI (memory notes, pause menu) calls this so the recipe board
        /// cannot be used underneath it.
        /// </summary>
        public void AddInputBlock()
        {
            inputBlocks++;
            RefreshInput();
        }

        public void RemoveInputBlock()
        {
            inputBlocks = Mathf.Max(0, inputBlocks - 1);
            RefreshInput();
        }

        private IEnumerator CaseRoutine(int index, Conversation before)
        {
            caseIndex = index;
            CustomerCase customerCase = cases[index];
            recipeOpen = false;
            RefreshInput();
            selection.ClearAll();

            if (customerLabel != null)
            {
                customerLabel.text = $"Customer {index + 1} of {cases.Length}: {customerCase.CustomerName}";
            }

            flow.BeginCustomerStory();
            var story = new List<Conversation> { before };
            story.AddRange(customerCase.Arrival);
            story.Add(customerCase.RecipeGuide);
            yield return Talk(story);

            StartCase(customerCase);
        }

        private void StartCase(CustomerCase customerCase)
        {
            validator.SetCase(customerCase);
            feedback.ResetAttempts();
            selection.ClearAll();

            if (resolutionPanel != null)
            {
                resolutionPanel.SetActive(false);
            }

            recipeOpen = true;
            RefreshInput();
            flow.MakeRecipeAvailable();
            CaseStarted?.Invoke(customerCase);
        }

        private IEnumerator FinishCaseRoutine()
        {
            yield return Talk(CurrentCase.Resolution);

            if (caseIndex + 1 < cases.Length)
            {
                yield return CaseRoutine(caseIndex + 1, null);
                yield break;
            }

            yield return Talk(new[] { ending });
            flow.ShowEnding();
        }

        private void HandleFirstMistake(RecipeCategory category)
        {
            CaseClue clue = CurrentCase?.GetClue(category);
            if (clue != null)
            {
                StartCoroutine(MistakeRoutine(clue.WrongReaction));
            }
        }

        private void HandleRepeatedMistake(RecipeCategory category)
        {
            CaseClue clue = CurrentCase?.GetClue(category);
            if (clue != null)
            {
                StartCoroutine(MistakeRoutine(clue.WrongReaction, clue.DirectHint));
            }
        }

        private IEnumerator MistakeRoutine(params Conversation[] reaction)
        {
            recipeOpen = false;
            RefreshInput();
            yield return Talk(reaction);
            recipeOpen = true;
            RefreshInput();
        }

        /// <summary>Raises the dialogue box, plays each non-empty conversation, then lowers it.</summary>
        private IEnumerator Talk(IEnumerable<Conversation> conversations)
        {
            var toPlay = new List<Conversation>();
            foreach (Conversation conversation in conversations)
            {
                if (conversation != null && conversation.Lines.Count > 0)
                {
                    toPlay.Add(conversation);
                }
            }

            if (toPlay.Count == 0 || DM == null || talking)
            {
                yield break;
            }

            talking = true;
            DM.Rise();
            foreach (Conversation conversation in toPlay)
            {
                yield return DM.ConversationStart(conversation);
            }

            DM.Drop();
            yield return new WaitForSeconds(0.5f);
            talking = false;
        }

        private void HandleRecipeCorrect()
        {
            SolvedCount++;
            recipeOpen = false;
            RefreshInput();
            flow.BeginResolution();

            if (resolutionText != null)
            {
                // With resolution dialogue the story is told there; the summary is the fallback.
                resolutionText.text = CurrentCase.Resolution.Count > 0 ? string.Empty : CurrentCase.ResolutionSummary;
            }

            if (continueLabel != null)
            {
                continueLabel.text = "CONTINUE";
            }

            if (resolutionPanel != null)
            {
                resolutionPanel.SetActive(true);
            }
            else
            {
                StartCoroutine(FinishCaseRoutine());
            }

            CaseSolved?.Invoke(CurrentCase);
        }

        private void RefreshInput()
        {
            bool inputEnabled = recipeOpen && inputBlocks == 0;

            if (dragDrop != null)
            {
                dragDrop.enabled = inputEnabled;
            }

            foreach (CanvasGroup group in recipeControls)
            {
                if (group != null)
                {
                    group.interactable = inputEnabled;
                }
            }
        }
    }
}
