using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen story panel shown behind the dialogue box: a colour fill, an
/// optional picture, and an optional title. Fades in and out.
/// </summary>
public class StoryPanel : MonoBehaviour
{
    [SerializeField] CanvasGroup group;
    [SerializeField] Image background;
    [SerializeField] Image picture;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] GameObject titleBand;
    [SerializeField] float fadeDuration = 0.6f;

    public bool IsShowing => gameObject.activeSelf;

    public IEnumerator Show(Color color, Sprite sprite, string titleText)
    {
        gameObject.SetActive(true);
        background.color = color;
        SetPicture(sprite);
        SetTitle(titleText);
        yield return Fade(0f, 1f);
    }

    /// <summary>Changes the picture mid-conversation; the opening title makes way for it.</summary>
    public void ChangePicture(Sprite sprite)
    {
        SetPicture(sprite);
        SetTitle(null);
    }

    void SetPicture(Sprite sprite)
    {
        picture.sprite = sprite;
        picture.enabled = sprite != null;
    }

    void SetTitle(string text)
    {
        title.text = text ?? string.Empty;
        if (titleBand != null)
            titleBand.SetActive(title.text.Length > 0 && picture.enabled);
    }

    public IEnumerator Hide()
    {
        if (!IsShowing)
            yield break;
        yield return Fade(1f, 0f);
        gameObject.SetActive(false);
    }

    IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        group.alpha = to;
    }
}
