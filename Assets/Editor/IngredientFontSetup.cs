#if UNITY_EDITOR
using TMPro;
using TheLastMooncake.Recipe;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Turns Assets/Fonts/Fredoka.ttf into a TextMesh Pro font and puts it on the
/// ingredient labels (the text inside each RecipeOption) in CafeTime.
/// </summary>
public static class IngredientFontSetup
{
    private const string FontPath = "Assets/Fonts/Fredoka.ttf";
    private const string FontAssetPath = "Assets/Fonts/Fredoka SDF.asset";
    private const string ScenePath = "Assets/Scenes/CafeTime.unity";
    private const string Characters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;!?'\"-()&/";

    [MenuItem("Tools/Last Mooncake/Apply Ingredient Font")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Stop Play mode before applying the ingredient font.");
            return;
        }

        TMP_FontAsset fontAsset = EnsureFontAsset();
        if (fontAsset == null)
        {
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (RecipeOption option in root.GetComponentsInChildren<RecipeOption>(true))
            {
                foreach (TMP_Text label in option.GetComponentsInChildren<TMP_Text>(true))
                {
                    Undo.RecordObject(label, "Apply Ingredient Font");
                    label.font = fontAsset;
                    label.fontSharedMaterial = fontAsset.material;
                    EditorUtility.SetDirty(label);
                    count++;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Applied {fontAsset.name} to {count} ingredient labels in {ScenePath}.");
    }

    private static TMP_FontAsset EnsureFontAsset()
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null)
        {
            return existing;
        }

        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null)
        {
            Debug.LogError($"{FontPath} is missing.");
            return null;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
        fontAsset.name = "Fredoka SDF";
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

        // The atlas texture and material have to live inside the font asset to be saved.
        fontAsset.atlasTextures[0].name = "Fredoka SDF Atlas";
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        fontAsset.material.name = "Fredoka SDF Material";
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        fontAsset.TryAddCharacters(Characters);
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        return fontAsset;
    }
}
#endif
