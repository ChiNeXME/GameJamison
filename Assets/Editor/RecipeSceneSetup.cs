#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
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
        SetWorldPosition(objects, "Lotus", new Vector3(-6f, 3f, 0f));
        SetWorldPosition(objects, "RedBean", new Vector3(-6f, 1.5f, 0f));
        SetWorldPosition(objects, "SaltedYolk", new Vector3(-6f, 0f, 0f));
        SetWorldPosition(objects, "Osmanthus", new Vector3(-6f, -1.5f, 0f));
        SetWorldPosition(objects, "Sesame", new Vector3(-6f, -3f, 0f));

        SetWorldPosition(objects, "FillingTarget", new Vector3(2f, 2.5f, 0f));
        SetWorldPosition(objects, "CentreTarget", new Vector3(2f, 0.5f, 0f));
        SetWorldPosition(objects, "FinishTarget", new Vector3(2f, -1.5f, 0f));

        Canvas canvas = RequireComponent<Canvas>(objects, "Canvas");
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = RequireOrAdd<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        SetRect(objects, "CentreControls", new Vector2(0.5f, 0f), new Vector2(-320f, 110f), new Vector2(280f, 130f));
        SetRect(objects, "SweetnessControls", new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(480f, 130f));
        SetRect(objects, "SubmitRecipeButton", new Vector2(0.5f, 0f), new Vector2(340f, 110f), new Vector2(250f, 70f));
        SetRect(objects, "LeaveEmptyButton", new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(240f, 55f));
        SetRect(objects, "LessSugarButton", new Vector2(0.5f, 0.5f), new Vector2(-115f, -20f), new Vector2(210f, 55f));
        SetRect(objects, "RegularSugarButton", new Vector2(0.5f, 0.5f), new Vector2(115f, -20f), new Vector2(210f, 55f));
        SetRect(objects, "CorrectPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 180f));
        SetRect(objects, "WrongPanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 180f));
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
