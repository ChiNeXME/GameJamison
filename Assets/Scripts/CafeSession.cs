using System;
using TMPro;
using TheLastMooncake.Customers;
using TheLastMooncake.Recipe;
using UnityEngine;
using System.Collections;

namespace TheLastMooncake.Flow
{
    /// <summary>
    /// Runs the customers in order. Each case opens the recipe board; a correct
    /// recipe shows the resolution panel, and finishing the last case shows the ending.
    /// </summary>
    public sealed class CafeSession : MonoBehaviour
    {
        [SerializeField] private DialogueHolder dialogueHolder;
        [SerializeField] private DialogueManager DM;    
        [SerializeField] private CustomerCase[] cases = Array.Empty<CustomerCase>();
        [SerializeField] private GameFlowController flow = null;
        [SerializeField] private RecipeSelection selection = null;
        [SerializeField] private RecipeValidator validator = null;
        [SerializeField] private RecipeFeedback feedback = null;
        [SerializeField] private DragDrop dragDrop = null;
        [SerializeField] private CanvasGroup[] recipeControls = Array.Empty<CanvasGroup>();

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI customerLabel = null;
        [SerializeField] private GameObject resolutionPanel = null;
        [SerializeField] private TextMeshProUGUI resolutionText = null;
        [SerializeField] private TextMeshProUGUI continueLabel = null;
        

        private int caseIndex = -1;
        private int inputBlocks;
        private bool recipeOpen;

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
            }
        }

        private void OnDisable()
        {
            if (feedback != null)
            {
                feedback.RecipeCorrect -= HandleRecipeCorrect;
            }
        }

        private void Start()
        {
            if (cases.Length == 0)
            {
                Debug.LogError("CafeSession has no customer cases assigned.", this);
                return;
            }

            PreCase(0, 0);
        }

        public void ContinueToNextCase()
        {
            if (caseIndex + 1 < cases.Length)
            {
                PreCase(caseIndex+1, caseIndex+1);
                return;
            }

            if (resolutionPanel != null)
            {
                resolutionPanel.SetActive(false);
            }

            flow.ShowEnding();
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

        private void StartCase(int index)
        {
            caseIndex = index;
            CustomerCase customerCase = cases[index];

            validator.SetCase(customerCase);
            feedback.ResetAttempts();
            selection.ClearAll();

            if (resolutionPanel != null)
            {
                resolutionPanel.SetActive(false);
            }

            if (customerLabel != null)
            {
                customerLabel.text = $"Customer {index + 1} of {cases.Length}: {customerCase.CustomerName}";
            }

            // The customer's dialogue will play here once it exists (GameState.CustomerStory);
            // until then the recipe opens straight away.
            recipeOpen = true;
            RefreshInput();
            flow.MakeRecipeAvailable();
            CaseStarted?.Invoke(customerCase);
        }

        public void PreCase(int ConvoIndex, int Index)
        {
            StartCoroutine(PreCaseRoutine(ConvoIndex, Index));
        }

        private IEnumerator PreCaseRoutine(int ConvoIndex, int Index)
        {
            DM.Rise();
            Conversation Convo = dialogueHolder.conversations[ConvoIndex];
            yield return DM.ConversationStart(Convo);
            DM.Drop();
            yield return new WaitForSeconds(0.5f);
            StartCase(Index);
        }

        private void HandleRecipeCorrect()
        {
            SolvedCount++;
            recipeOpen = false;
            RefreshInput();
            flow.BeginResolution();

            if (resolutionText != null)
            {
                resolutionText.text = CurrentCase.ResolutionSummary;
            }

            if (continueLabel != null)
            {
                continueLabel.text = caseIndex + 1 < cases.Length ? "NEXT CUSTOMER" : "FINISH";
            }

            if (resolutionPanel != null)
            {
                resolutionPanel.SetActive(true);
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
