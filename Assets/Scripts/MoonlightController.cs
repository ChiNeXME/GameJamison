using TheLastMooncake.Customers;
using TheLastMooncake.Recipe;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheLastMooncake.Flow
{
    /// <summary>
    /// Moonlight as progress feedback: starts faint, flickers on a wrong recipe,
    /// and brightens a step (with a pulse) for every customer helped.
    /// </summary>
    public sealed class MoonlightController : MonoBehaviour
    {
        [SerializeField] private CafeSession session = null;
        [SerializeField] private RecipeFeedback feedback = null;
        [SerializeField] private SpriteRenderer moon = null;
        [SerializeField] private Light2D moonLight = null;

        [Header("Look")]
        [SerializeField] private Color dimMoonColor = new(0.42f, 0.46f, 0.66f, 1f);
        [SerializeField] private Color brightMoonColor = new(1f, 0.96f, 0.82f, 1f);
        [SerializeField, Min(0f)] private float dimIntensity = 0.25f;
        [SerializeField, Min(0f)] private float brightIntensity = 1.4f;
        [SerializeField, Range(0f, 1f)] private float startingBrightness = 0f;

        [Header("Timing")]
        [SerializeField, Min(0.01f)] private float brightenSpeed = 0.5f;
        [SerializeField, Min(0.01f)] private float flickerDuration = 0.6f;
        [SerializeField, Min(0.01f)] private float pulseDuration = 1.2f;
        [SerializeField, Min(0f)] private float pulseStrength = 1.2f;

        private float brightness;
        private float targetBrightness;
        private float flickerTimer;
        private float pulseTimer;

        private void Awake()
        {
            brightness = targetBrightness = startingBrightness;
            Apply();
        }

        private void OnEnable()
        {
            if (session != null)
            {
                session.CaseSolved += HandleCaseSolved;
            }

            if (feedback != null)
            {
                feedback.RecipeWrong += HandleRecipeWrong;
            }
        }

        private void OnDisable()
        {
            if (session != null)
            {
                session.CaseSolved -= HandleCaseSolved;
            }

            if (feedback != null)
            {
                feedback.RecipeWrong -= HandleRecipeWrong;
            }
        }

        private void Update()
        {
            brightness = Mathf.MoveTowards(brightness, targetBrightness, brightenSpeed * Time.deltaTime);
            flickerTimer = Mathf.Max(0f, flickerTimer - Time.deltaTime);
            pulseTimer = Mathf.Max(0f, pulseTimer - Time.deltaTime);
            Apply();
        }

        private void HandleCaseSolved(CustomerCase customerCase)
        {
            targetBrightness = session.CaseCount > 0
                ? Mathf.Clamp01((float)session.SolvedCount / session.CaseCount)
                : 1f;
            pulseTimer = pulseDuration;
        }

        private void HandleRecipeWrong()
        {
            flickerTimer = flickerDuration;
        }

        private void Apply()
        {
            float flicker = 0f;
            if (flickerTimer > 0f)
            {
                float fade = flickerTimer / flickerDuration;
                flicker = fade * (0.5f + 0.5f * Mathf.Sin(Time.time * 45f)) * 0.6f;
            }

            float pulse = pulseTimer > 0f ? Mathf.Sin(pulseTimer / pulseDuration * Mathf.PI) * pulseStrength : 0f;

            if (moonLight != null)
            {
                float intensity = Mathf.Lerp(dimIntensity, brightIntensity, brightness);
                moonLight.intensity = intensity * (1f - flicker) + pulse;
            }

            if (moon != null)
            {
                Color color = Color.Lerp(dimMoonColor, brightMoonColor, brightness);
                moon.color = Color.Lerp(color, dimMoonColor * 0.6f, flicker);
            }
        }
    }
}
