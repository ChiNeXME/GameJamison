using UnityEngine;

namespace TheLastMooncake.Recipe
{
    public sealed class RecipeDropTarget : MonoBehaviour
    {
        [SerializeField] private RecipeCategory acceptedCategory = RecipeCategory.Filling;
        [SerializeField] private Transform snapPoint = null;

        public RecipeCategory AcceptedCategory => acceptedCategory;
        public RecipeOption CurrentOption { get; private set; }

        public bool TryPlace(RecipeOption option)
        {
            if (option == null || option.Category != acceptedCategory)
            {
                return false;
            }

            if (option.CurrentTarget != null && option.CurrentTarget != this)
            {
                option.CurrentTarget.Remove(option);
            }

            if (CurrentOption != null && CurrentOption != option)
            {
                CurrentOption.ResetToHome();
            }

            CurrentOption = option;
            Vector3 destination = snapPoint != null ? snapPoint.position : transform.position;
            destination.z = option.transform.position.z;
            option.PlaceIn(this, destination);
            return true;
        }

        public void Remove(RecipeOption option)
        {
            if (CurrentOption == option)
            {
                CurrentOption = null;
            }
        }

        public void Clear()
        {
            CurrentOption?.ResetToHome();
            CurrentOption = null;
        }
    }
}
