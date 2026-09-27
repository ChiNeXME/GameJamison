using System;
using System.Collections.Generic;
using TheLastMooncake.Recipe;
using UnityEngine;

namespace TheLastMooncake.Customers
{
    /// <summary>
    /// One recipe decision in a customer's story: the correct answer, the note
    /// Artemis records, and the feedback shown after a first and second mistake
    /// (text for the notebook, plus optional dialogue).
    /// </summary>
    [Serializable]
    public sealed class CaseClue
    {
        [SerializeField] private RecipeCategory category = RecipeCategory.Filling;
        [SerializeField] private RecipeChoice answer = RecipeChoice.None;
        [SerializeField, TextArea] private string memoryNote = string.Empty;
        [SerializeField, TextArea] private string hint = string.Empty;
        [SerializeField, TextArea] private string correction = string.Empty;
        [Tooltip("Customer's reaction when this choice is wrong.")]
        [SerializeField] private Conversation wrongReaction = null;
        [Tooltip("Artemis's direct hint, added after the second mistake.")]
        [SerializeField] private Conversation directHint = null;

        public CaseClue(
            RecipeCategory category,
            RecipeChoice answer,
            string memoryNote,
            string hint,
            string correction)
        {
            this.category = category;
            this.answer = answer;
            this.memoryNote = memoryNote;
            this.hint = hint;
            this.correction = correction;
        }

        public RecipeCategory Category => category;
        public RecipeChoice Answer => answer;
        public string MemoryNote => memoryNote;
        public string Hint => hint;
        public string Correction => correction;
        public Conversation WrongReaction => wrongReaction;
        public Conversation DirectHint => directHint;

#if UNITY_EDITOR
        public void EditorSetText(string newMemoryNote, string newCorrection)
        {
            memoryNote = newMemoryNote;
            correction = newCorrection;
        }

        public void EditorSetDialogue(Conversation newWrongReaction, Conversation newDirectHint)
        {
            wrongReaction = newWrongReaction;
            directHint = newDirectHint;
        }
#endif
    }

    [CreateAssetMenu(fileName = "CustomerCase", menuName = "Last Mooncake/Customer Case")]
    public sealed class CustomerCase : ScriptableObject
    {
        [SerializeField] private string customerName = "New Customer";
        [SerializeField] private CaseClue[] clues = Array.Empty<CaseClue>();
        [SerializeField, TextArea(3, 6)] private string resolutionSummary = string.Empty;

        [Header("Dialogue")]
        [Tooltip("Played in order before the recipe opens (each customer's arrival).")]
        [SerializeField] private Conversation[] arrival = Array.Empty<Conversation>();
        [Tooltip("Baker's lines right before the recipe board opens.")]
        [SerializeField] private Conversation recipeGuide = null;
        [Tooltip("Played in order after the correct mooncake.")]
        [SerializeField] private Conversation[] resolution = Array.Empty<Conversation>();

        public string CustomerName => customerName;
        public IReadOnlyList<Conversation> Arrival => arrival;
        public Conversation RecipeGuide => recipeGuide;
        public IReadOnlyList<Conversation> Resolution => resolution;
        public IReadOnlyList<CaseClue> Clues => clues;
        public string ResolutionSummary => resolutionSummary;

        public CaseClue GetClue(RecipeCategory category)
        {
            foreach (CaseClue clue in clues)
            {
                if (clue != null && clue.Category == category)
                {
                    return clue;
                }
            }

            return null;
        }

        public RecipeChoice GetAnswer(RecipeCategory category)
        {
            CaseClue clue = GetClue(category);
            return clue != null ? clue.Answer : RecipeChoice.None;
        }

#if UNITY_EDITOR
        public void EditorSetup(string newCustomerName, string newResolutionSummary, params CaseClue[] newClues)
        {
            customerName = newCustomerName;
            resolutionSummary = newResolutionSummary;
            clues = newClues;
        }

        public void EditorSetDialogue(Conversation[] newArrival, Conversation newRecipeGuide, Conversation[] newResolution)
        {
            arrival = newArrival;
            recipeGuide = newRecipeGuide;
            resolution = newResolution;
        }
#endif

        private void OnValidate()
        {
            foreach (RecipeCategory category in RecipeRules.AllCategories)
            {
                CaseClue clue = GetClue(category);
                if (clue == null)
                {
                    Debug.LogWarning($"{name} has no clue for {category}.", this);
                }
                else if (!RecipeRules.IsChoiceForCategory(category, clue.Answer))
                {
                    Debug.LogWarning($"{name}: {clue.Answer} is not a valid answer for {category}.", this);
                }
            }
        }
    }
}
