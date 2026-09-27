#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using TheLastMooncake.Flow;
using TheLastMooncake.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the end credits screen: the ending cutscene behind a dark veil, the
/// title and closing line, one card per team member, and the buttons.
/// Rebuilt from scratch each time, so edit the credits here.
/// </summary>
public static class EndingCreditsSetup
{
    private const string BackdropArt = "Assets/Art/Cutscenes/EndingCutscene.png";

    private static readonly (string name, string role)[] Credits =
    {
        ("Hoji", "Artist, Model"),
        ("dasfsadwasd", "Programmer, Artist"),
        ("chinexme", "Programmer, UI"),
        ("lbummer", "Music Composer & Sound Effects"),
    };

    private static readonly Color Gold = new(1f, 0.8f, 0.42f);
    private static readonly Color PaleGold = new(1f, 0.93f, 0.78f);
    private static readonly Color Moonlight = new(0.82f, 0.85f, 1f);
    private static readonly Color ButtonColor = new(0.58f, 0.29f, 0.12f, 1f);

    public static GameObject Build(Transform canvas, GameObject sessionObject, GameFlowController flow)
    {
        EndingScreen ending = sessionObject.GetComponent<EndingScreen>();
        if (ending == null)
        {
            ending = Undo.AddComponent<EndingScreen>(sessionObject);
        }

        Transform existing = canvas.Find("EndingPanel");
        GameObject panel = existing != null
            ? existing.gameObject
            : new GameObject("EndingPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvas, false);
        panel.layer = canvas.gameObject.layer;
        Stretch(panel.GetComponent<RectTransform>());
        panel.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.09f, 1f);

        // Start clean: the layout is fully generated below.
        for (int index = panel.transform.childCount - 1; index >= 0; index--)
        {
            Object.DestroyImmediate(panel.transform.GetChild(index).gameObject);
        }

        // Ending cutscene fills the screen, dimmed so the text reads on top.
        Image backdrop = CreateImage(panel.transform, "Backdrop", Color.white);
        backdrop.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackdropArt);
        backdrop.enabled = backdrop.sprite != null;
        Stretch(backdrop.rectTransform);
        AspectRatioFitter fitter = backdrop.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = 16f / 9f;
        Stretch(CreateImage(panel.transform, "Veil", new Color(0.02f, 0.03f, 0.1f, 0.74f)).rectTransform);

        var reveal = new List<CanvasGroup>();

        // Title and closing line.
        CanvasGroup titleGroup = CreateGroup(panel.transform, "TitleGroup");
        Text(titleGroup.transform, "Title", "THE LAST MOONCAKE", new Vector2(0f, 360f), new Vector2(1400f, 110f), 88f, Gold, FontStyles.Bold, 12f);
        Text(titleGroup.transform, "Quote", "\"The past does not return. It leaves us enough light to follow.\"", new Vector2(0f, 280f), new Vector2(1400f, 60f), 30f, Moonlight, FontStyles.Italic);
        reveal.Add(titleGroup);

        // "CREDITS" with a thin gold rule either side.
        CanvasGroup header = CreateGroup(panel.transform, "CreditsHeader");
        Text(header.transform, "Label", "CREDITS", new Vector2(0f, 175f), new Vector2(400f, 50f), 30f, Gold, FontStyles.Bold, 18f);
        Rule(header.transform, "RuleLeft", new Vector2(-260f, 175f));
        Rule(header.transform, "RuleRight", new Vector2(260f, 175f));
        reveal.Add(header);

        // One card per person, two columns.
        for (int index = 0; index < Credits.Length; index++)
        {
            float x = index % 2 == 0 ? -330f : 330f;
            float y = index < 2 ? 50f : -110f;
            CanvasGroup card = CreateGroup(panel.transform, $"Credit_{Credits[index].name}");
            Image cardBack = CreateImage(card.transform, "Card", new Color(1f, 0.85f, 0.55f, 0.08f));
            Place(cardBack.rectTransform, new Vector2(x, y), new Vector2(560f, 130f));
            Text(card.transform, "Name", Credits[index].name, new Vector2(x, y + 22f), new Vector2(540f, 60f), 46f, PaleGold, FontStyles.Bold);
            Text(card.transform, "Role", Credits[index].role, new Vector2(x, y - 30f), new Vector2(540f, 40f), 26f, Moonlight, FontStyles.Normal, 2f);
            reveal.Add(card);
        }

        CanvasGroup thanks = CreateGroup(panel.transform, "Thanks");
        Text(thanks.transform, "Label", "Thank you for playing.", new Vector2(0f, -250f), new Vector2(1000f, 50f), 32f, Gold, FontStyles.Italic);
        reveal.Add(thanks);

        CanvasGroup buttons = CreateGroup(panel.transform, "Buttons");
        CreateButton(buttons.transform, "PlayAgainButton", "PLAY AGAIN", new Vector2(-200f, -370f), ending.PlayAgain);
        CreateButton(buttons.transform, "TitleButton", "RETURN TO TITLE", new Vector2(200f, -370f), ending.ReturnToTitle);
        reveal.Add(buttons);

        var serialized = new SerializedObject(ending);
        serialized.FindProperty("flow").objectReferenceValue = flow;
        serialized.FindProperty("panel").objectReferenceValue = panel;
        SerializedProperty order = serialized.FindProperty("revealInOrder");
        order.arraySize = reveal.Count;
        for (int index = 0; index < reveal.Count; index++)
        {
            order.GetArrayElementAtIndex(index).objectReferenceValue = reveal[index];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(panel);
        return panel;
    }

    private static CanvasGroup CreateGroup(Transform parent, string name)
    {
        var groupObject = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        groupObject.transform.SetParent(parent, false);
        groupObject.layer = parent.gameObject.layer;
        Stretch(groupObject.GetComponent<RectTransform>());
        return groupObject.GetComponent<CanvasGroup>();
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        imageObject.layer = parent.gameObject.layer;
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI Text(
        Transform parent, string name, string text, Vector2 position, Vector2 size,
        float fontSize, Color color, FontStyles style, float characterSpacing = 0f)
    {
        var textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        textObject.layer = parent.gameObject.layer;
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = color;
        label.characterSpacing = characterSpacing;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        Place(label.rectTransform, position, size);
        return label;
    }

    private static void Rule(Transform parent, string name, Vector2 position)
    {
        Place(CreateImage(parent, name, new Color(Gold.r, Gold.g, Gold.b, 0.6f)).rectTransform, position, new Vector2(300f, 2f));
    }

    private static void CreateButton(Transform parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        Image image = CreateImage(parent, name, ButtonColor);
        image.raycastTarget = true;
        Place(image.rectTransform, position, new Vector2(340f, 78f));
        UnityEngine.UI.Button button = image.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        Text(image.transform, "Label", label, Vector2.zero, new Vector2(340f, 78f), 30f, Color.white, FontStyles.Bold);
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
#endif
