using System;
using System.Collections.Generic;
using TMPro;
using TheLastMooncake.Customers;
using TheLastMooncake.Flow;
using TheLastMooncake.Recipe;
using UnityEngine;
using UnityEngine.UI;

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
        private GameObject reminder;
        private TextMeshProUGUI reminderText;
        private CanvasGroup reminderGroup;

        public bool IsOpen => panel != null && panel.activeSelf;
        void Awake()
        {
            // SettingsCanvas comes from the main menu; it is missing when CafeTime is played directly.
            GameObject settings = GameObject.Find("SettingsCanvas");
            SS = settings != null ? settings.GetComponent<SettingsScript>() : null;
            CreateReminder();
        }

        private void Update()
        {
            if (reminder != null && reminder.activeSelf)
            {
                reminderGroup.alpha = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 4f);
            }
        }

        // A pulsing note under the MEMORIES button reminding players to open the notebook.
        private void CreateReminder()
        {
            Transform button = buttonLabel != null ? buttonLabel.transform.parent : null;
            if (button == null)
            {
                return;
            }

            reminder = new GameObject("NotesReminder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            reminder.transform.SetParent(button, false);
            var rect = (RectTransform)reminder.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(0f, -10f);
            rect.sizeDelta = new Vector2(430f, 56f);

            Image background = reminder.GetComponent<Image>();
            background.color = new Color(0.1f, 0.06f, 0.03f, 0.8f);
            background.raycastTarget = false;
            reminderGroup = reminder.GetComponent<CanvasGroup>();
            reminderGroup.blocksRaycasts = false;
            reminderGroup.interactable = false;

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(reminder.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 4f);
            textRect.offsetMax = new Vector2(-12f, -4f);

            reminderText = textObject.GetComponent<TextMeshProUGUI>();
            reminderText.font = buttonLabel.font;
            reminderText.fontSize = 22f;
            reminderText.enableAutoSizing = true;
            reminderText.fontSizeMin = 14f;
            reminderText.fontSizeMax = 22f;
            reminderText.alignment = TextAlignmentOptions.Center;
            reminderText.color = new Color(1f, 0.92f, 0.72f);
            reminderText.raycastTarget = false;

            reminder.SetActive(false);
        }

        private void ShowReminder(bool show)
        {
            if (reminder == null)
            {
                return;
            }

            if (show)
            {
                bool autoNotes = SS == null || SS.IsAutoNotesOnBool;
                reminderText.text = autoNotes
                    ? "New memories! Check your Memory Notes."
                    : "Open your Memory Notes to write down clues!";
            }

            reminder.SetActive(show && !IsOpen);
        }

        private void HandleCustomerArriving(CustomerCase customerCase) => ShowReminder(true);

        private void HandleCaseSolved(CustomerCase customerCase) => ShowReminder(false);

        private void OnEnable()
        {
            if (session != null)
            {
                session.CaseStarted += HandleCaseStarted;
                session.CustomerArriving += HandleCustomerArriving;
                session.CaseSolved += HandleCaseSolved;
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
                session.CustomerArriving -= HandleCustomerArriving;
                session.CaseSolved -= HandleCaseSolved;
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
                SetNoteRowsVisible(true);
            }
            else if (SS && !SS.IsAutoNotesOnBool)//Enable manual + Disable Auto etc
            {
                ManualNotes.SetActive(true);
                AutoNotes.SetActive(false);
                SetNoteRowsVisible(false);
            }

            panel.SetActive(true);
            session?.AddInputBlock();
            hasUnread = false;
            ShowReminder(false);
            Refresh();
        }

        // The auto note rows sit beside AutoNotes in the scene rather than inside it, so they are toggled separately.
        private void SetNoteRowsVisible(bool visible)
        {
            foreach (TextMeshProUGUI row in noteRows)
            {
                if (row != null)
                {
                    row.gameObject.SetActive(visible);
                }
            }
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
            ShowReminder(true);
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
            ShowReminder(true);
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
