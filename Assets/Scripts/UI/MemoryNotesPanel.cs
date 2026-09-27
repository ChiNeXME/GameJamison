using System;
using System.Collections.Generic;
using TMPro;
using TheLastMooncake.Customers;
using TheLastMooncake.Flow;
using TheLastMooncake.Recipe;
using UnityEngine;
using UnityEditor.SettingsManagement;

namespace TheLastMooncake.UI
{
    /// <summary>
    /// Artemis's notebook. Lists the current customer's memory notes, marks the
    /// relevant note after a wrong recipe, and adds the direct correction after a second one.
    /// </summary>
    public sealed class MemoryNotesPanel : MonoBehaviour
    {
        [SerializeField] private CafeSession session = null;
        [SerializeField] private RecipeFeedback feedback = null;
        [SerializeField] private GameObject panel = null;
        [SerializeField] private TextMeshProUGUI heading = null;
        [SerializeField] private TextMeshProUGUI[] noteRows = Array.Empty<TextMeshProUGUI>();
        [SerializeField] private TextMeshProUGUI buttonLabel = null;
        [SerializeField] private Color noteColor = new(0.25f, 0.15f, 0.08f);
        [SerializeField] private Color highlightColor = new(0.72f, 0.26f, 0.04f);
        SettingsScript SS;
        [SerializeField] GameObject ManualNotes;
        [SerializeField] GameObject AutoNotes;

        private readonly Dictionary<RecipeCategory, string> corrections = new();
        private RecipeCategory? highlighted;
        private bool hasUnread;

        public bool IsOpen => panel != null && panel.activeSelf;
        void Start()
        {
            SS = GameObject.Find("SettingsCanvas").GetComponent<SettingsScript>();
        }

        private void OnEnable()
        {
            if (session != null)
            {
                session.CaseStarted += HandleCaseStarted;
            }

            if (feedback != null)
            {
                feedback.HintRequested += HandleHint;
                feedback.ExplicitCorrectionRequested += HandleCorrection;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (session != null)
            {
                session.CaseStarted -= HandleCaseStarted;
            }

            if (feedback != null)
            {
                feedback.HintRequested -= HandleHint;
                feedback.ExplicitCorrectionRequested -= HandleCorrection;
            }
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (IsOpen || panel == null)
            {
                return;
            }

            if (SS && SS.IsAutoNotesOnBool) //Disable manual + Enable Auto etc
            {
                ManualNotes.SetActive(false);
                AutoNotes.SetActive(true);
            }
            else if (SS && !SS.IsAutoNotesOnBool)//Enable manual + Disable Auto etc
            {
                ManualNotes.SetActive(true);
                AutoNotes.SetActive(false);
            }

            panel.SetActive(true);
            session?.AddInputBlock();
            hasUnread = false;
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            panel.SetActive(false);
            session?.RemoveInputBlock();
        }

        private void HandleCaseStarted(CustomerCase customerCase)
        {
            corrections.Clear();
            highlighted = null;
            hasUnread = true;
            Refresh();
        }

        private void HandleHint(RecipeCategory category)
        {
            highlighted = category;
            hasUnread = true;
            Refresh();
        }

        private void HandleCorrection(RecipeCategory category)
        {
            highlighted = category;
            CaseClue clue = CurrentCase?.GetClue(category);
            if (clue != null)
            {
                corrections[category] = clue.Correction;
            }

            hasUnread = true;
            Refresh();
        }

        private CustomerCase CurrentCase => session != null ? session.CurrentCase : null;

        private void Refresh()
        {
            CustomerCase customerCase = CurrentCase;

            if (heading != null)
            {
                heading.text = customerCase != null
                    ? $"ARTEMIS'S MEMORY NOTES\n<size=70%>{customerCase.CustomerName}</size>"
                    : "ARTEMIS'S MEMORY NOTES";
            }

            for (int index = 0; index < noteRows.Length; index++)
            {
                TextMeshProUGUI row = noteRows[index];
                if (row == null)
                {
                    continue;
                }

                if (customerCase == null || index >= customerCase.Clues.Count)
                {
                    row.text = string.Empty;
                    continue;
                }

                CaseClue clue = customerCase.Clues[index];
                bool isHighlighted = highlighted == clue.Category;
                string text = (isHighlighted ? "> " : "- ") + clue.MemoryNote;
                if (corrections.TryGetValue(clue.Category, out string correction))
                {
                    text += $"\n   <i>{correction}</i>";
                }

                row.text = text;
                row.color = isHighlighted ? highlightColor : noteColor;
                row.fontStyle = isHighlighted ? FontStyles.Bold : FontStyles.Normal;
            }

            if (buttonLabel != null)
            {
                buttonLabel.text = hasUnread && !IsOpen ? "MEMORIES (!)" : "MEMORIES";
            }
        }
    }
}
