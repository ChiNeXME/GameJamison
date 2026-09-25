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
            return choices.TryGetValue(category, out choice);
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
                    RecipeChanged?.Invoke(option.Category, RecipeChoice.None);
                }

                return;
            }

            choices[option.Category] = option.Choice;
            RecipeChanged?.Invoke(option.Category, option.Choice);
        }
    }
}
