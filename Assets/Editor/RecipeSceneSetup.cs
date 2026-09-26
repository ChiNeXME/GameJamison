#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TheLastMooncake.Customers;
using TheLastMooncake.Flow;
using TheLastMooncake.Recipe;
using TheLastMooncake.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;

public static class RecipeSceneSetup
{
    private const string ScenePath = "Assets/Scenes/CafeTime.unity";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string CasesFolder = "Assets/Data/Customers";

    [MenuItem("Tools/Last Mooncake/Configure Recipe Scene")]
    public static void ConfigureRecipeScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Dictionary<string, GameObject> objects = IndexScene(scene);

        int ingredientLayer = RequireLayer("Ingredient");
        int targetLayer = RequireLayer("RecipeDropTarget");

        DragDrop dragDrop = RequireComponent<DragDrop>(objects, "DragDropSystem");
        RecipeSelection selection = RequireComponent<RecipeSelection>(objects, "RecipeSystem");
        RecipeValidator validator = RequireComponent<RecipeValidator>(objects, "RecipeSystem");
        RecipeFeedback feedback = RequireComponent<RecipeFeedback>(objects, "RecipeSystem");
        GameFlowController flow = RequireComponent<GameFlowController>(objects, "RecipeSystem");
        GameObject sessionObject = FindOrCreateChild(RequireObject(objects, "Systems").transform, "CafeSession");
        CafeSession session = RequireOrAdd<CafeSession>(sessionObject);

        RemoveMisplacedChoiceButtons(scene);

        ConfigureIngredient(objects, "Lotus", ingredientLayer, RecipeCategory.Filling, RecipeChoice.Lotus);
        ConfigureIngredient(objects, "RedBean", ingredientLayer, RecipeCategory.Filling, RecipeChoice.RedBean);
        ConfigureIngredient(objects, "SaltedYolk", ingredientLayer, RecipeCategory.Centre, RecipeChoice.SaltedYolk);
        ConfigureIngredient(objects, "Osmanthus", ingredientLayer, RecipeCategory.Finish, RecipeChoice.Osmanthus);
        ConfigureIngredient(objects, "Sesame", ingredientLayer, RecipeCategory.Finish, RecipeChoice.Sesame);

        RecipeDropTarget filling = ConfigureTarget(objects, "FillingTarget", targetLayer, RecipeCategory.Filling);
        RecipeDropTarget centre = ConfigureTarget(objects, "CentreTarget", targetLayer, RecipeCategory.Centre);
        RecipeDropTarget finish = ConfigureTarget(objects, "FinishTarget", targetLayer, RecipeCategory.Finish);

        SetObjectReference(dragDrop, "draggableLayer", null, 1 << ingredientLayer);
        SetObjectReference(dragDrop, "dropTargetLayer", null, 1 << targetLayer);
        SetBoolean(dragDrop, "snapToTarget", true);

        SetObjectReference(selection, "dragDrop", dragDrop);
        SetObjectArray(selection, "targets", filling, centre, finish);
        SetObjectReference(validator, "selection", selection);
        SetObjectReference(feedback, "validator", validator);
        SetObjectReference(feedback, "selection", selection);

        ConfigureChoiceButton(objects, "LeaveEmptyButton", selection, RecipeCategory.Centre, RecipeChoice.NoYolk);
        ConfigureChoiceButton(objects, "LessSugarButton", selection, RecipeCategory.Sweetness, RecipeChoice.LowSweetness);
        ConfigureChoiceButton(objects, "RegularSugarButton", selection, RecipeCategory.Sweetness, RecipeChoice.RegularSweetness);
        ConfigureSubmitButton(objects, "SubmitRecipeButton", validator);
        ConfigureFeedback(objects, feedback);
        ConfigureHintDisplay(objects, selection, feedback, session);
        ArrangeGreybox(objects);
        ConfigureCafeSession(objects, session, EnsureCustomerCases(), flow, selection, validator, feedback, dragDrop);

        GameObject player = FindOptional(objects, "Player");
        if (player != null)
        {
            var movement = player.GetComponent<TheLastMooncake.Player.PlayerController2D>();
            if (movement != null)
            {
                movement.enabled = false;
                EditorUtility.SetDirty(movement);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        CreateMainMenuScene();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(ScenePath, true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("Recipe scene and main menu configured. MainMenu is now the first build scene.");
    }

    private static void RemoveMisplacedChoiceButtons(Scene scene)
    {
        var allowedNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "LeaveEmptyButton",
            "LessSugarButton",
            "RegularSugarButton"
        };

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (RecipeChoiceButton choiceButton in root.GetComponentsInChildren<RecipeChoiceButton>(true))
            {
                if (!allowedNames.Contains(choiceButton.gameObject.name))
                {
                    Undo.DestroyObjectImmediate(choiceButton);
                }
            }
        }
    }

    private static Dictionary<string, GameObject> IndexScene(Scene scene)
    {
        var result = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                result.TryAdd(transform.name, transform.gameObject);
            }
        }

        return result;
    }

    private static int RequireLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            throw new InvalidOperationException($"Required layer '{layerName}' does not exist.");
        }

        return layer;
    }

    private static T RequireComponent<T>(Dictionary<string, GameObject> objects, string objectName)
        where T : Component
    {
        GameObject gameObject = RequireObject(objects, objectName);
        T component = gameObject.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(gameObject);
    }

    private static GameObject RequireObject(Dictionary<string, GameObject> objects, string objectName)
    {
        if (!objects.TryGetValue(objectName, out GameObject gameObject))
        {
            throw new InvalidOperationException($"Required scene object '{objectName}' was not found.");
        }

        return gameObject;
    }

    private static GameObject FindOptional(Dictionary<string, GameObject> objects, string objectName)
    {
        objects.TryGetValue(objectName, out GameObject gameObject);
        return gameObject;
    }

    private static void ConfigureIngredient(
        Dictionary<string, GameObject> objects,
        string name,
        int layer,
        RecipeCategory category,
        RecipeChoice choice)
    {
        GameObject ingredient = RequireObject(objects, name);
        ingredient.layer = layer;
        RequireOrAdd<SpriteRenderer>(ingredient);
        RequireOrAdd<BoxCollider2D>(ingredient);
        RecipeOption option = RequireOrAdd<RecipeOption>(ingredient);
        SetEnum(option, "category", (int)category);
        SetEnum(option, "choice", (int)choice);
        EditorUtility.SetDirty(ingredient);
    }

    private static RecipeDropTarget ConfigureTarget(
        Dictionary<string, GameObject> objects,
        string name,
        int layer,
        RecipeCategory category)
    {
        GameObject targetObject = RequireObject(objects, name);
        targetObject.layer = layer;
        RequireOrAdd<BoxCollider2D>(targetObject);
        RecipeDropTarget target = RequireOrAdd<RecipeDropTarget>(targetObject);
        SetEnum(target, "acceptedCategory", (int)category);
        EditorUtility.SetDirty(targetObject);
        return target;
    }

    private static void ConfigureChoiceButton(
        Dictionary<string, GameObject> objects,
        string name,
        RecipeSelection selection,
        RecipeCategory category,
        RecipeChoice choice)
    {
        GameObject buttonObject = RequireObject(objects, name);
        Button button = RequireOrAdd<Button>(buttonObject);
        RecipeChoiceButton choiceButton = RequireOrAdd<RecipeChoiceButton>(buttonObject);
        SetObjectReference(choiceButton, "selection", selection);
        SetEnum(choiceButton, "category", (int)category);
        SetEnum(choiceButton, "choice", (int)choice);
        SetObjectReference(choiceButton, "button", button);

        RemoveAllPersistentListeners(button.onClick);
        UnityEventTools.AddPersistentListener(button.onClick, choiceButton.Select);
        EditorUtility.SetDirty(button);
    }

    private static void ConfigureSubmitButton(
        Dictionary<string, GameObject> objects,
        string name,
        RecipeValidator validator)
    {
        Button button = RequireOrAdd<Button>(RequireObject(objects, name));
        RemoveAllPersistentListeners(button.onClick);
        UnityEventTools.AddPersistentListener(button.onClick, validator.Submit);
        EditorUtility.SetDirty(button);
    }

    private static void ConfigureFeedback(
        Dictionary<string, GameObject> objects,
        RecipeFeedback feedback)
    {
        GameObject correctPanel = RequireObject(objects, "CorrectPanel");
        GameObject wrongPanel = RequireObject(objects, "WrongPanel");
        StyleFeedbackPanel(correctPanel, new Color(0.18f, 0.42f, 0.2f, 0.96f), "RECIPE CORRECT");
        StyleFeedbackPanel(wrongPanel, new Color(0.48f, 0.16f, 0.13f, 0.96f), "TRY ANOTHER COMBINATION");
        correctPanel.SetActive(false);
        wrongPanel.SetActive(false);

        // CafeSession owns the correct-recipe flow (resolution panel, next customer).
        RemoveAllPersistentListeners(feedback.OnCorrectRecipe);

        RemoveAllPersistentListeners(feedback.OnWrongRecipe);
        UnityEventTools.AddBoolPersistentListener(feedback.OnWrongRecipe, wrongPanel.SetActive, true);
        EditorUtility.SetDirty(feedback);
    }

    private static void ConfigureHintDisplay(
        Dictionary<string, GameObject> objects,
        RecipeSelection selection,
        RecipeFeedback feedback,
        CafeSession session)
    {
        GameObject recipeSystem = RequireObject(objects, "RecipeSystem");
        RecipeHintDisplay display = RequireOrAdd<RecipeHintDisplay>(recipeSystem);
        Canvas canvas = RequireComponent<Canvas>(objects, "Canvas");

        Transform existing = canvas.transform.Find("HintPanel");
        GameObject panel = existing != null
            ? existing.gameObject
            : new GameObject("HintPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.16f, 0.10f, 0.07f, 0.96f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -125f);
        rect.sizeDelta = new Vector2(760f, 105f);

        TextMeshProUGUI label = CreateOverlayText(
            panel.transform,
            "HintText",
            string.Empty,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(700f, 85f),
            27f,
            TextAlignmentOptions.Center);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        SetObjectReference(display, "feedback", feedback);
        SetObjectReference(display, "selection", selection);
        SetObjectReference(display, "session", session);
        SetObjectReference(display, "panel", panel);
        SetObjectReference(display, "feedbackPanel", RequireObject(objects, "WrongPanel"));
        SetObjectReference(display, "message", label);
        panel.SetActive(false);
        EditorUtility.SetDirty(display);
    }

    private static CustomerCase[] EnsureCustomerCases()
    {
        EnsureFolder("Assets", "Data");
        EnsureFolder("Assets/Data", "Customers");

        // Assets are only created when missing so later edits in the Inspector are kept.
        CustomerCase lina = EnsureCase($"{CasesFolder}/01_Lina.asset", customerCase => customerCase.EditorSetup(
            "Lina Chen",
            "Lina ties the box with its faded ribbon. The mooncake cannot repair five silent years, but it gives her a reason to knock on her father's door.",
            new CaseClue(RecipeCategory.Filling, RecipeChoice.RedBean,
                "Lina stole warm red bean paste from her father's spoon.",
                "Tutorial: Choose the filling Lina named directly.",
                "Use red bean paste."),
            new CaseClue(RecipeCategory.Centre, RecipeChoice.NoYolk,
                "Her father left out the yolk she disliked.",
                "Tutorial: Choose what her father deliberately left out.",
                "Leave the centre empty."),
            new CaseClue(RecipeCategory.Sweetness, RecipeChoice.LowSweetness,
                "He reduced the sugar because he listened.",
                "Tutorial: Match the sweetness to her memory.",
                "Choose less sugar."),
            new CaseClue(RecipeCategory.Finish, RecipeChoice.Osmanthus,
                "A tiny osmanthus flower marked the cake as hers.",
                "Tutorial: Finish the cake with her flower mark.",
                "Finish with osmanthus.")));

        CustomerCase siblings = EnsureCase($"{CasesFolder}/03_MeiAndJian.asset", customerCase => customerCase.EditorSetup(
            "Mei & Jian",
            "The mold glows, but its crack remains. The mooncake tastes almost like Grandmother's kitchen. Mei and Jian write down what remains together.",
            new CaseClue(RecipeCategory.Filling, RecipeChoice.Lotus,
                "Mei ground lotus seeds beside Grandmother.",
                "Hint: Think about which seeds were ground into the filling.",
                "Try lotus paste for the filling."),
            new CaseClue(RecipeCategory.Centre, RecipeChoice.SaltedYolk,
                "Jian placed one little moon in the middle.",
                "Hint: Remember the little moon placed in the middle.",
                "Place one salted egg yolk in the centre."),
            new CaseClue(RecipeCategory.Sweetness, RecipeChoice.LowSweetness,
                "Only a little sugar. The filling should not taste like syrup.",
                "Hint: The filling should not taste like syrup.",
                "Choose less sugar."),
            new CaseClue(RecipeCategory.Finish, RecipeChoice.Osmanthus,
                "Osmanthus filled the kitchen while the cakes cooled.",
                "Hint: Recall the floral scent while the cakes cooled.",
                "Finish with osmanthus.")));

        return new[] { lina, siblings };
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static CustomerCase EnsureCase(string path, Action<CustomerCase> initialise)
    {
        CustomerCase existing = AssetDatabase.LoadAssetAtPath<CustomerCase>(path);
        if (existing != null)
        {
            return existing;
        }

        CustomerCase created = ScriptableObject.CreateInstance<CustomerCase>();
        initialise(created);
        AssetDatabase.CreateAsset(created, path);
        return created;
    }

    private static void ConfigureCafeSession(
        Dictionary<string, GameObject> objects,
        CafeSession session,
        CustomerCase[] defaultCases,
        GameFlowController flow,
        RecipeSelection selection,
        RecipeValidator validator,
        RecipeFeedback feedback,
        DragDrop dragDrop)
    {
        Transform canvas = RequireComponent<Canvas>(objects, "Canvas").transform;
        GameObject sessionObject = session.gameObject;

        // Customer order: keep whatever was set in the Inspector, otherwise use the defaults.
        var serializedSession = new SerializedObject(session);
        if (serializedSession.FindProperty("cases").arraySize == 0)
        {
            SetObjectArray(session, "cases", defaultCases);
        }

        SetObjectReference(session, "flow", flow);
        SetObjectReference(session, "selection", selection);
        SetObjectReference(session, "validator", validator);
        SetObjectReference(session, "feedback", feedback);
        SetObjectReference(session, "dragDrop", dragDrop);
        SetObjectArray(
            session,
            "recipeControls",
            RequireOrAdd<CanvasGroup>(RequireObject(objects, "CentreControls")),
            RequireOrAdd<CanvasGroup>(RequireObject(objects, "SweetnessControls")),
            RequireOrAdd<CanvasGroup>(RequireObject(objects, "SubmitRecipeButton")));

        // Top-of-screen labels.
        TextMeshProUGUI customerLabel = CreateOverlayText(canvas, "CustomerLabel", string.Empty, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(760f, 50f), 28f, TextAlignmentOptions.Center);
        TextMeshProUGUI pauseHint = CreateOverlayText(canvas, "PauseHint", "Esc: Pause", new Vector2(0f, 1f), new Vector2(190f, -150f), new Vector2(330f, 40f), 20f, TextAlignmentOptions.Left);
        pauseHint.fontStyle = FontStyles.Normal;
        pauseHint.color = new Color(0.85f, 0.85f, 0.95f, 0.8f);
        SetObjectReference(session, "customerLabel", customerLabel);

        // Recipe status readout.
        RecipeStatusDisplay status = RequireOrAdd<RecipeStatusDisplay>(RequireObject(objects, "RecipeSystem"));
        TextMeshProUGUI statusLabel = CreateOverlayText(canvas, "RecipeStatus", string.Empty, new Vector2(0.5f, 1f), new Vector2(0f, -178f), new Vector2(1100f, 80f), 24f, TextAlignmentOptions.Center);
        statusLabel.fontStyle = FontStyles.Normal;
        statusLabel.color = new Color(1f, 0.92f, 0.78f);
        SetObjectReference(status, "selection", selection);
        SetObjectReference(status, "validator", validator);
        SetObjectReference(status, "label", statusLabel);

        ConfigureResolutionPanel(objects, session);
        GameObject notesPanel = ConfigureMemoryNotes(objects, canvas, session, feedback, out MemoryNotesPanel notes);
        GameObject endingPanel = ConfigureEnding(canvas, sessionObject, flow);
        GameObject pausePanel = ConfigurePauseMenu(canvas, sessionObject, session, notes);

        notesPanel.transform.SetAsLastSibling();
        endingPanel.transform.SetAsLastSibling();
        pausePanel.transform.SetAsLastSibling();
        notesPanel.SetActive(false);
        endingPanel.SetActive(false);
        pausePanel.SetActive(false);

        ConfigureMoon(objects, session, feedback);
        EditorUtility.SetDirty(session);
    }

    private static void ConfigureResolutionPanel(Dictionary<string, GameObject> objects, CafeSession session)
    {
        GameObject panel = RequireObject(objects, "CorrectPanel");
        SetRect(objects, "CorrectPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880f, 360f));

        Transform titleTransform = panel.transform.Find("Text (TMP)");
        if (titleTransform != null)
        {
            SetAnchoredRect(titleTransform.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(820f, 60f));
        }

        TextMeshProUGUI summary = CreateOverlayText(panel.transform, "Summary", string.Empty, new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(800f, 170f), 26f, TextAlignmentOptions.Center);
        summary.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        summary.fontStyle = FontStyles.Normal;
        summary.color = Color.white;

        TextMeshProUGUI continueLabel = CreateButton(panel.transform, "ContinueButton", "NEXT CUSTOMER", new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(340f, 72f), session.ContinueToNextCase);

        SetObjectReference(session, "resolutionPanel", panel);
        SetObjectReference(session, "resolutionText", summary);
        SetObjectReference(session, "continueLabel", continueLabel);
        panel.SetActive(false);
    }

    private static GameObject ConfigureMemoryNotes(
        Dictionary<string, GameObject> objects,
        Transform canvas,
        CafeSession session,
        RecipeFeedback feedback,
        out MemoryNotesPanel notes)
    {
        notes = RequireOrAdd<MemoryNotesPanel>(session.gameObject);

        GameObject backdrop = CreatePanel(canvas, "MemoryNotesPanel", new Color(0f, 0f, 0f, 0.55f));
        StretchToParent(backdrop.GetComponent<RectTransform>());

        GameObject card = CreatePanel(backdrop.transform, "Card", new Color(0.93f, 0.86f, 0.7f, 1f));
        SetAnchoredRect(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920f, 640f));

        Color ink = new(0.25f, 0.15f, 0.08f);
        TextMeshProUGUI heading = CreateOverlayText(card.transform, "Heading", "ARTEMIS'S MEMORY NOTES", new Vector2(0.5f, 1f), new Vector2(0f, -25f), new Vector2(840f, 100f), 36f, TextAlignmentOptions.Center);
        heading.color = ink;

        var rows = new TextMeshProUGUI[4];
        for (int index = 0; index < rows.Length; index++)
        {
            TextMeshProUGUI row = CreateOverlayText(card.transform, $"Note{index}", string.Empty, new Vector2(0.5f, 1f), new Vector2(0f, -145f - index * 100f), new Vector2(820f, 95f), 26f, TextAlignmentOptions.TopLeft);
            row.fontStyle = FontStyles.Normal;
            row.color = ink;
            rows[index] = row;
        }

        CreateButton(card.transform, "CloseButton", "CLOSE", new Vector2(0.5f, 0f), new Vector2(0f, 25f), new Vector2(240f, 64f), notes.Close);

        GameObject memoriesButton = RequireObject(objects, "MemoriesButton");
        Button button = RequireOrAdd<Button>(memoriesButton);
        button.targetGraphic = memoriesButton.GetComponent<Image>();
        RemoveAllPersistentListeners(button.onClick);
        UnityEventTools.AddPersistentListener(button.onClick, notes.Toggle);
        Transform buttonLabel = memoriesButton.transform.Find("Label");

        SetObjectReference(notes, "session", session);
        SetObjectReference(notes, "feedback", feedback);
        SetObjectReference(notes, "panel", backdrop);
        SetObjectReference(notes, "heading", heading);
        SetObjectArray(notes, "noteRows", rows);
        SetObjectReference(notes, "buttonLabel", buttonLabel != null ? buttonLabel.GetComponent<TextMeshProUGUI>() : null);
        EditorUtility.SetDirty(notes);
        return backdrop;
    }

    private static GameObject ConfigureEnding(Transform canvas, GameObject sessionObject, GameFlowController flow)
    {
        EndingScreen ending = RequireOrAdd<EndingScreen>(sessionObject);

        GameObject panel = CreatePanel(canvas, "EndingPanel", new Color(0.06f, 0.07f, 0.13f, 0.97f));
        StretchToParent(panel.GetComponent<RectTransform>());

        TextMeshProUGUI title = CreateOverlayText(panel.transform, "Title", "THE MOON SHINES AGAIN", new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(1200f, 110f), 64f, TextAlignmentOptions.Center);
        title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        title.color = new Color(1f, 0.82f, 0.45f);

        TextMeshProUGUI body = CreateOverlayText(panel.transform, "Body", "Full moonlight reaches the bakery floor.\nThe baker places a tiny cat-shaped mooncake on the windowsill, and Artemis takes one bite.", new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1100f, 140f), 30f, TextAlignmentOptions.Center);
        body.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        body.fontStyle = FontStyles.Normal;
        body.color = new Color(0.9f, 0.9f, 1f);

        TextMeshProUGUI credits = CreateOverlayText(panel.transform, "Credits", "CREDITS\n<size=80%>Add your team's names here</size>", new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(1000f, 160f), 30f, TextAlignmentOptions.Center);
        credits.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        credits.color = new Color(0.78f, 0.78f, 0.9f);

        CreateButton(panel.transform, "PlayAgainButton", "PLAY AGAIN", new Vector2(0.5f, 0.5f), new Vector2(-200f, -260f), new Vector2(340f, 78f), ending.PlayAgain);
        CreateButton(panel.transform, "TitleButton", "RETURN TO TITLE", new Vector2(0.5f, 0.5f), new Vector2(200f, -260f), new Vector2(340f, 78f), ending.ReturnToTitle);

        SetObjectReference(ending, "flow", flow);
        SetObjectReference(ending, "panel", panel);
        EditorUtility.SetDirty(ending);
        return panel;
    }

    private static GameObject ConfigurePauseMenu(Transform canvas, GameObject sessionObject, CafeSession session, MemoryNotesPanel notes)
    {
        PauseMenu pause = RequireOrAdd<PauseMenu>(sessionObject);

        GameObject panel = CreatePanel(canvas, "PausePanel", new Color(0f, 0f, 0f, 0.72f));
        StretchToParent(panel.GetComponent<RectTransform>());

        TextMeshProUGUI title = CreateOverlayText(panel.transform, "Title", "PAUSED", new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(600f, 100f), 60f, TextAlignmentOptions.Center);
        title.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        CreateButton(panel.transform, "ResumeButton", "RESUME", new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(360f, 78f), pause.Resume);
        CreateButton(panel.transform, "RestartButton", "RESTART", new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(360f, 78f), pause.Restart);
        CreateButton(panel.transform, "TitleButton", "RETURN TO TITLE", new Vector2(0.5f, 0.5f), new Vector2(0f, -140f), new Vector2(360f, 78f), pause.ReturnToTitle);

        SetObjectReference(pause, "panel", panel);
        SetObjectReference(pause, "session", session);
        SetObjectReference(pause, "notes", notes);
        EditorUtility.SetDirty(pause);
        return panel;
    }

    private static void ConfigureMoon(Dictionary<string, GameObject> objects, CafeSession session, RecipeFeedback feedback)
    {
        GameObject moon = FindOptional(objects, "Moon");
        if (moon == null)
        {
            moon = new GameObject("Moon");
            Undo.RegisterCreatedObjectUndo(moon, "Create Moon");
        }

        moon.transform.position = new Vector3(4.6f, 3.6f, 0f);

        SpriteRenderer renderer = RequireOrAdd<SpriteRenderer>(moon);
        Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        renderer.sprite = circle != null ? circle : RequireObject(objects, "FillingTarget").GetComponent<SpriteRenderer>().sprite;
        renderer.sortingOrder = -5;
        float diameter = 1.3f;
        float spriteWidth = Mathf.Max(0.0001f, renderer.sprite.bounds.size.x);
        moon.transform.localScale = Vector3.one * (diameter / spriteWidth);

        GameObject lightObject = FindOrCreateChild(moon.transform, "MoonLight");
        lightObject.transform.localPosition = Vector3.zero;
        lightObject.transform.localScale = Vector3.one;
        Light2D light = RequireOrAdd<Light2D>(lightObject);
        light.lightType = Light2D.LightType.Point;
        light.color = new Color(0.62f, 0.68f, 1f);
        light.pointLightInnerRadius = 0.5f;
        light.pointLightOuterRadius = 9f;

        // Light every sorting layer so the moonlight reaches the whole counter.
        var serializedLight = new SerializedObject(light);
        SerializedProperty layers = serializedLight.FindProperty("m_ApplyToSortingLayers");
        if (layers != null)
        {
            SortingLayer[] sortingLayers = SortingLayer.layers;
            layers.arraySize = sortingLayers.Length;
            for (int index = 0; index < sortingLayers.Length; index++)
            {
                layers.GetArrayElementAtIndex(index).intValue = sortingLayers[index].id;
            }

            serializedLight.ApplyModifiedPropertiesWithoutUndo();
        }

        MoonlightController controller = RequireOrAdd<MoonlightController>(moon);
        SetObjectReference(controller, "session", session);
        SetObjectReference(controller, "feedback", feedback);
        SetObjectReference(controller, "moon", renderer);
        SetObjectReference(controller, "moonLight", light);
        EditorUtility.SetDirty(moon);
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new(name);
        child.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(child, $"Create {name}");
        return child;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject panel = existing != null
            ? existing.gameObject
            : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        RequireOrAdd<Image>(panel).color = color;
        EditorUtility.SetDirty(panel);
        return panel;
    }

    private static TextMeshProUGUI CreateButton(
        Transform parent,
        string name,
        string labelText,
        Vector2 anchor,
        Vector2 position,
        Vector2 size,
        UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreatePanel(parent, name, new Color(0.58f, 0.29f, 0.12f, 1f));
        SetAnchoredRect(buttonObject.GetComponent<RectTransform>(), anchor, position, size);

        Button button = RequireOrAdd<Button>(buttonObject);
        button.targetGraphic = buttonObject.GetComponent<Image>();
        RemoveAllPersistentListeners(button.onClick);
        UnityEventTools.AddPersistentListener(button.onClick, action);

        TextMeshProUGUI label = CreateOverlayText(buttonObject.transform, "Label", labelText, new Vector2(0.5f, 0.5f), Vector2.zero, size, 30f, TextAlignmentOptions.Center);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        label.color = Color.white;
        EditorUtility.SetDirty(buttonObject);
        return label;
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void CreateMainMenuScene()
    {
        Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.055f, 0.07f, 0.13f);
        camera.orthographic = true;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject background = CreateMenuPanel(canvasObject.transform, "Background", new Color(0.08f, 0.07f, 0.12f, 1f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        TextMeshProUGUI title = CreateOverlayText(background.transform, "Title", "THE LAST\nMOONCAKE", new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(900f, 250f), 84f, TextAlignmentOptions.Center);
        title.color = new Color(1f, 0.78f, 0.35f);
        TextMeshProUGUI subtitle = CreateOverlayText(background.transform, "Subtitle", "A story about recipes, memory, and moonlight", new Vector2(0.5f, 0.5f), new Vector2(0f, 45f), new Vector2(900f, 65f), 26f, TextAlignmentOptions.Center);
        subtitle.fontStyle = FontStyles.Normal;
        subtitle.color = new Color(0.78f, 0.78f, 0.9f);

        GameObject controllerObject = new("MainMenuController", typeof(TheLastMooncake.UI.MainMenuController));
        TheLastMooncake.UI.MainMenuController controller = controllerObject.GetComponent<TheLastMooncake.UI.MainMenuController>();
        CreateMenuButton(background.transform, "PlayButton", "PLAY", new Vector2(0f, -85f), controller.Play);
        CreateMenuButton(background.transform, "QuitButton", "QUIT", new Vector2(0f, -190f), controller.Quit);

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        EditorSceneManager.SaveScene(menuScene, MainMenuScenePath);
    }

    private static GameObject CreateMenuPanel(Transform parent, string name, Color color)
    {
        GameObject panel = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static void CreateMenuButton(Transform parent, string name, string labelText, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreateMenuPanel(parent, name, new Color(0.58f, 0.29f, 0.12f, 1f));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(360f, 78f);

        Button button = buttonObject.AddComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, action);
        TextMeshProUGUI label = CreateOverlayText(buttonObject.transform, "Label", labelText, new Vector2(0.5f, 0.5f), Vector2.zero, rect.sizeDelta, 32f, TextAlignmentOptions.Center);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        label.color = Color.white;
    }

    private static void ArrangeGreybox(Dictionary<string, GameObject> objects)
    {
        ArrangeIngredient(objects, "Lotus", "LOTUS PASTE", new Vector3(-7f, 2.8f, 0f), new Color(0.95f, 0.78f, 0.35f));
        ArrangeIngredient(objects, "RedBean", "RED BEAN PASTE", new Vector3(-7f, 1.4f, 0f), new Color(0.55f, 0.12f, 0.13f));
        ArrangeIngredient(objects, "SaltedYolk", "SALTED EGG YOLK", new Vector3(-7f, 0f, 0f), new Color(1f, 0.45f, 0.08f));
        ArrangeIngredient(objects, "Osmanthus", "OSMANTHUS", new Vector3(-7f, -1.4f, 0f), new Color(0.95f, 0.65f, 0.12f));
        ArrangeIngredient(objects, "Sesame", "SESAME", new Vector3(-7f, -2.8f, 0f), new Color(0.25f, 0.25f, 0.28f));

        ArrangeTarget(objects, "FillingTarget", "FILLING", new Vector3(-2.8f, 0.2f, 0f));
        ArrangeTarget(objects, "CentreTarget", "CENTRE", new Vector3(3.6f, 0.2f, 0f));
        ArrangeTarget(objects, "FinishTarget", "FINISH", new Vector3(0.4f, -2.2f, 0f));
        CreateMoldPlaceholder(objects);

        Canvas canvas = RequireComponent<Canvas>(objects, "Canvas");
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = RequireOrAdd<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        SetRect(objects, "CentreControls", new Vector2(0.5f, 0f), new Vector2(-300f, 95f), new Vector2(300f, 120f));
        SetRect(objects, "SweetnessControls", new Vector2(0.5f, 0f), new Vector2(40f, 95f), new Vector2(480f, 120f));
        SetRect(objects, "SubmitRecipeButton", new Vector2(0.5f, 0f), new Vector2(430f, 95f), new Vector2(320f, 82f));
        SetRect(objects, "LeaveEmptyButton", new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(240f, 55f));
        SetRect(objects, "LessSugarButton", new Vector2(0.5f, 0.5f), new Vector2(-115f, -20f), new Vector2(210f, 55f));
        SetRect(objects, "RegularSugarButton", new Vector2(0.5f, 0.5f), new Vector2(115f, -20f), new Vector2(210f, 55f));
        SetRect(objects, "CorrectPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 180f));
        SetRect(objects, "WrongPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 180f));

        CreateOverlayText(canvas.transform, "GameTitle", "THE LAST MOONCAKE", new Vector2(0f, 1f), new Vector2(190f, -65f), new Vector2(330f, 80f), 34f, TextAlignmentOptions.Left);
        CreateOverlayText(canvas.transform, "InstructionText", "Build the mooncake they remember.", new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(760f, 70f), 34f, TextAlignmentOptions.Center);
        CreateMemoryPlaceholder(canvas.transform);
    }

    private static void ArrangeIngredient(
        Dictionary<string, GameObject> objects,
        string name,
        string label,
        Vector3 position,
        Color color)
    {
        GameObject ingredient = RequireObject(objects, name);
        SetWorldPosition(objects, name, position);
        SpriteRenderer renderer = RequireOrAdd<SpriteRenderer>(ingredient);
        if (renderer.sprite == null)
        {
            renderer.sprite = RequireObject(objects, "FillingTarget").GetComponent<SpriteRenderer>().sprite;
        }
        renderer.color = color;
        renderer.sortingOrder = 2;
        ingredient.transform.localScale = new Vector3(1.25f, 1.05f, 1f);
        CreateWorldLabel(ingredient.transform, label, new Vector3(0f, -0.75f, 0f), 2.2f);
    }

    private static void ArrangeTarget(
        Dictionary<string, GameObject> objects,
        string name,
        string label,
        Vector3 position)
    {
        GameObject target = RequireObject(objects, name);
        SetWorldPosition(objects, name, position);
        SpriteRenderer renderer = RequireOrAdd<SpriteRenderer>(target);
        renderer.color = new Color(0.35f, 0.2f, 0.12f, 0.82f);
        renderer.sortingOrder = 3;
        target.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
        CreateWorldLabel(target.transform, label, Vector3.zero, 2.6f);
    }

    private static void CreateMoldPlaceholder(Dictionary<string, GameObject> objects)
    {
        GameObject recipeArea = RequireObject(objects, "RecipeArea");
        Transform existing = recipeArea.transform.Find("MooncakeMoldPlaceholder");
        GameObject mold = existing != null ? existing.gameObject : new GameObject("MooncakeMoldPlaceholder");
        mold.transform.SetParent(recipeArea.transform, false);
        mold.transform.position = new Vector3(0.4f, 0.35f, 0f);
        mold.transform.localScale = new Vector3(3.6f, 2.8f, 1f);

        SpriteRenderer renderer = RequireOrAdd<SpriteRenderer>(mold);
        renderer.sprite = RequireObject(objects, "FillingTarget").GetComponent<SpriteRenderer>().sprite;
        renderer.color = new Color(0.34f, 0.18f, 0.09f, 0.65f);
        renderer.sortingOrder = 0;
        CreateWorldLabel(mold.transform, "MOONCAKE MOLD", Vector3.zero, 1.3f);
        EditorUtility.SetDirty(mold);
    }

    private static void CreateWorldLabel(Transform parent, string text, Vector3 localPosition, float fontSize)
    {
        Transform existing = parent.Find("GreyboxLabel");
        GameObject labelObject = existing != null ? existing.gameObject : new GameObject("GreyboxLabel");
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = localPosition;
        labelObject.transform.localScale = Vector3.one;

        TextMeshPro label = RequireOrAdd<TextMeshPro>(labelObject);
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.9f, 0.7f);
        label.sortingOrder = 20;
        label.rectTransform.sizeDelta = new Vector2(5f, 1.2f);
        EditorUtility.SetDirty(labelObject);
    }

    private static TextMeshProUGUI CreateOverlayText(
        Transform canvas,
        string name,
        string text,
        Vector2 anchor,
        Vector2 position,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        Transform existing = canvas.Find(name);
        GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(canvas, false);
        TextMeshProUGUI label = RequireOrAdd<TextMeshProUGUI>(textObject);
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = alignment;
        label.color = new Color(1f, 0.82f, 0.47f);

        RectTransform rect = label.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        EditorUtility.SetDirty(textObject);
        return label;
    }

    private static void CreateMemoryPlaceholder(Transform canvas)
    {
        Transform existing = canvas.Find("MemoriesButton");
        GameObject buttonObject = existing != null
            ? existing.gameObject
            : new GameObject("MemoriesButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas, false);
        Image image = RequireOrAdd<Image>(buttonObject);
        image.color = new Color(0.22f, 0.12f, 0.08f, 0.9f);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-45f, -45f);
        rect.sizeDelta = new Vector2(210f, 105f);

        TextMeshProUGUI label = CreateOverlayText(buttonObject.transform, "Label", "MEMORIES", new Vector2(0.5f, 0.5f), Vector2.zero, rect.sizeDelta, 27f, TextAlignmentOptions.Center);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        EditorUtility.SetDirty(buttonObject);
    }

    private static void StyleFeedbackPanel(GameObject panel, Color color, string message)
    {
        Image image = RequireOrAdd<Image>(panel);
        image.color = color;
        TextMeshProUGUI label = panel.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = message;
            label.fontSize = 34f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
        }
    }

    private static void SetWorldPosition(
        Dictionary<string, GameObject> objects,
        string name,
        Vector3 position)
    {
        Transform transform = RequireObject(objects, name).transform;
        transform.position = position;
        transform.localScale = Vector3.one;
        EditorUtility.SetDirty(transform);
    }

    private static void SetRect(
        Dictionary<string, GameObject> objects,
        string name,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        RectTransform rect = RequireObject(objects, name).GetComponent<RectTransform>();
        if (rect == null)
        {
            throw new InvalidOperationException($"UI object '{name}' needs a RectTransform.");
        }

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        EditorUtility.SetDirty(rect);
    }

    private static T RequireOrAdd<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(gameObject);
    }

    private static void RemoveAllPersistentListeners(UnityEngine.Events.UnityEventBase unityEvent)
    {
        for (int index = unityEvent.GetPersistentEventCount() - 1; index >= 0; index--)
        {
            UnityEventTools.RemovePersistentListener(unityEvent, index);
        }
    }

    private static void SetObjectReference(
        UnityEngine.Object target,
        string propertyName,
        UnityEngine.Object value,
        int? layerMask = null)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' was not found on {target.GetType().Name}.");
        }

        if (layerMask.HasValue)
        {
            property.intValue = layerMask.Value;
        }
        else
        {
            property.objectReferenceValue = value;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectArray(UnityEngine.Object target, string propertyName, params UnityEngine.Object[] values)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (int index = 0; index < values.Length; index++)
        {
            property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetEnum(UnityEngine.Object target, string propertyName, int value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).enumValueIndex = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBoolean(UnityEngine.Object target, string propertyName, bool value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
