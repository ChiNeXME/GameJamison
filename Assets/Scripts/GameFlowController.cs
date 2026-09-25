using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastMooncake.Flow
{
    public enum GameState
    {
        Intro,
        CustomerStory,
        ReviewNotes,
        RecipeAvailable,
        RecipeAttempt,
        Resolution,
        Ending
    }

    [Serializable]
    public sealed class GameStateEvent : UnityEvent<GameState>
    {
    }

    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private GameState initialState = GameState.Intro;
        [SerializeField] private GameStateEvent onStateChanged = new();

        public GameState CurrentState { get; private set; }
        public event Action<GameState> StateChanged;
        private bool hasStarted;

        private void Start()
        {
            SetState(initialState);
        }

        public void SetState(GameState nextState)
        {
            if (hasStarted && CurrentState == nextState)
            {
                return;
            }

            hasStarted = true;
            CurrentState = nextState;
            StateChanged?.Invoke(CurrentState);
            onStateChanged?.Invoke(CurrentState);
        }

        public void BeginCustomerStory() => SetState(GameState.CustomerStory);
        public void ReviewNotes() => SetState(GameState.ReviewNotes);
        public void MakeRecipeAvailable() => SetState(GameState.RecipeAvailable);
        public void BeginRecipeAttempt() => SetState(GameState.RecipeAttempt);
        public void BeginResolution() => SetState(GameState.Resolution);
        public void ShowEnding() => SetState(GameState.Ending);
        public void RestartFlow() => SetState(GameState.Intro);
    }
}
