using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastMooncake.Recipe
{
    public sealed class RecipeSelection : MonoBehaviour
    {
        [SerializeField] private DragDrop dragDrop = null;
        [SerializeField] private RecipeDropTarget[] targets = Array.Empty<RecipeDropTarget>();

        private readonly Dictionary<RecipeCategory, RecipeChoice> choices = new();

        public event Action<RecipeCategory, RecipeChoice> RecipeChanged;

        private void OnEnable()
        {
            if (dragDrop != null)
            {
                dragDrop.IngredientDropped += HandleIngredientDropped;
            }
        }

        private void OnDisable()
        {
            if (dragDrop != null)
            {
                dragDrop.IngredientDropped -= HandleIngredientDropped;
            }
        }

        public bool TryGetChoice(RecipeCategory category, out RecipeChoice choice)
        {
            if (choices.TryGetValue(category, out choice))
            {
                return true;
            }

            choice = DefaultChoice(category);
            return choice != RecipeChoice.None;
        }

        /// <summary>
        /// The centre has no "leave empty" control: without a yolk it simply counts as empty.
        /// </summary>
        public static RecipeChoice DefaultChoice(RecipeCategory category)
        {
            return category == RecipeCategory.Centre ? RecipeChoice.NoYolk : RecipeChoice.None;
        }

        public bool SelectChoice(RecipeCategory category, RecipeChoice choice)
        {
            if (!RecipeRules.IsChoiceForCategory(category, choice))
            {
                Debug.LogWarning($"{choice} is not a valid choice for {category}.", this);
                return false;
            }

            RecipeDropTarget physicalTarget = FindTarget(category);
            physicalTarget?.Clear();
            SetChoice(category, choice);
            return true;
        }

        public void ClearAll()
        {
            if (targets != null)
            {
                foreach (RecipeDropTarget target in targets)
                {
                    target?.Clear();
                }
            }

            choices.Clear();
            RecipeChanged?.Invoke(RecipeCategory.Filling, RecipeChoice.None);
            RecipeChanged?.Invoke(RecipeCategory.Centre, DefaultChoice(RecipeCategory.Centre));
            RecipeChanged?.Invoke(RecipeCategory.Sweetness, RecipeChoice.None);
            RecipeChanged?.Invoke(RecipeCategory.Finish, RecipeChoice.None);
        }

        private void HandleIngredientDropped(GameObject ingredientObject, Collider2D targetCollider)
        {
            RecipeOption option = ingredientObject.GetComponentInParent<RecipeOption>();
            RecipeDropTarget target = targetCollider.GetComponentInParent<RecipeDropTarget>();

            if (option == null)
            {
                return;
            }

            if (target == null || !target.TryPlace(option))
            {
                bool wasSelected = option.CurrentTarget != null;
                option.ResetToHome();

                if (wasSelected)
                {
                    choices.Remove(option.Category);
                    RecipeChanged?.Invoke(option.Category, DefaultChoice(option.Category));
                }

                return;
            }

            SetChoice(option.Category, option.Choice);
        }

        private RecipeDropTarget FindTarget(RecipeCategory category)
        {
            if (targets == null)
            {
                return null;
            }

            foreach (RecipeDropTarget target in targets)
            {
                if (target != null && target.AcceptedCategory == category)
                {
                    return target;
                }
            }

            return null;
        }

        private void SetChoice(RecipeCategory category, RecipeChoice choice)
        {
            choices[category] = choice;
            RecipeChanged?.Invoke(category, choice);
        }
    }
}
