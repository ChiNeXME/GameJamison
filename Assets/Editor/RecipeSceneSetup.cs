#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TheLastMooncake.Flow;
using TheLastMooncake.Recipe;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RecipeSceneSetup
{
    private const string ScenePath = "Assets/Scenes/CafeTime.unity";

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

        ConfigureChoiceButton(objects, "LeaveEmptyButton", selection, RecipeCategory.Centre, RecipeChoice.NoYolk);
        ConfigureChoiceButton(objects, "LessSugarButton", selection, RecipeCategory.Sweetness, RecipeChoice.LowSweetness);
        ConfigureChoiceButton(objects, "RegularSugarButton", selection, RecipeCategory.Sweetness, RecipeChoice.RegularSweetness);
        ConfigureSubmitButton(objects, "SubmitRecipeButton", validator);
        ConfigureFeedback(objects, feedback, flow);
        ArrangeGreybox(objects);

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

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("The Last Mooncake recipe scene is configured. CafeTime is now the build scene.");
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
        RecipeFeedback feedback,
        GameFlowController flow)
    {
        GameObject correctPanel = RequireObject(objects, "CorrectPanel");
        GameObject wrongPanel = RequireObject(objects, "WrongPanel");
        StyleFeedbackPanel(correctPanel, new Color(0.18f, 0.42f, 0.2f, 0.96f), "RECIPE CORRECT");
        StyleFeedbackPanel(wrongPanel, new Color(0.48f, 0.16f, 0.13f, 0.96f), "TRY ANOTHER COMBINATION");
        correctPanel.SetActive(false);
        wrongPanel.SetActive(false);

        RemoveAllPersistentListeners(feedback.OnCorrectRecipe);
        UnityEventTools.AddBoolPersistentListener(feedback.OnCorrectRecipe, correctPanel.SetActive, true);
        UnityEventTools.AddPersistentListener(feedback.OnCorrectRecipe, flow.BeginResolution);

        RemoveAllPersistentListeners(feedback.OnWrongRecipe);
        UnityEventTools.AddBoolPersistentListener(feedback.OnWrongRecipe, wrongPanel.SetActive, true);
        EditorUtility.SetDirty(feedback);
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
