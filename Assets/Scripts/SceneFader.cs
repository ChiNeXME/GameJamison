using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fades to black between scenes. Creates itself on startup (no scene setup needed);
/// load scenes with SceneFader.LoadScene instead of SceneManager.LoadScene.
/// </summary>
public class SceneFader : MonoBehaviour
{
    const float FadeTime = 0.45f;

    static SceneFader instance;

    CanvasGroup group;
    bool loading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        var go = new GameObject("SceneFader", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneFader>();

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // above everything, including settings

        var black = new GameObject("Black", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        black.transform.SetParent(go.transform, false);
        var rect = (RectTransform)black.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        black.GetComponent<Image>().color = Color.black;

        // Start black so the first scene fades in too.
        instance.group = go.GetComponent<CanvasGroup>();
        instance.group.alpha = 1f;
        instance.group.blocksRaycasts = true;
        SceneManager.sceneLoaded += (scene, mode) => instance.FadeIn();
    }

    void FadeIn()
    {
        StopAllCoroutines();
        loading = false;
        StartCoroutine(Fade(0f));
    }

    /// <summary>True while the screen is (mostly) black from a scene transition.</summary>
    public static bool IsScreenCovered => instance != null && instance.group.alpha > 0.5f;

    public static void LoadScene(string sceneName)
    {
        if (instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (instance.loading)
            return;

        instance.StopAllCoroutines();
        instance.StartCoroutine(instance.FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        loading = true;
        yield return Fade(1f);
        SceneManager.LoadScene(sceneName);
    }

    // Unscaled time: the pause menu sets timeScale to 0.
    IEnumerator Fade(float target)
    {
        group.blocksRaycasts = true; // no clicks mid-fade
        float start = group.alpha;
        for (float t = 0f; t < FadeTime; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(start, target, t / FadeTime);
            yield return null;
        }

        group.alpha = target;
        group.blocksRaycasts = target > 0f;
    }
}
