using UnityEngine;

namespace TheLastMooncake.Recipe
{
    public sealed class RecipeOption : MonoBehaviour
    {
        [SerializeField] private RecipeCategory category = RecipeCategory.Filling;
        [SerializeField] private RecipeChoice choice = RecipeChoice.None;

        private Vector3 homePosition;

        public RecipeCategory Category => category;
        public RecipeChoice Choice => choice;
        public RecipeDropTarget CurrentTarget { get; private set; }

        private void Awake()
        {
            homePosition = transform.position;
        }

        internal void PlaceIn(RecipeDropTarget target, Vector3 position)
        {
            CurrentTarget = target;
            transform.position = position;
        }

        public void ResetToHome()
        {
            RecipeDropTarget previousTarget = CurrentTarget;
            CurrentTarget = null;
            previousTarget?.Remove(this);
            transform.position = homePosition;
        }
    }
}
