#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using TheLastMooncake.Customers;
using TheLastMooncake.Flow;
using TheLastMooncake.Recipe;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Imports Docs/Dialogue/00_FULL_SCRIPT.md into Conversation assets (one per story
/// beat), hooks them up to the customer cases, and wires the dialogue objects in
/// CafeTime. Re-run it whenever the script changes; the script is the source of truth.
/// </summary>
public static class DialogueImporter
{
    private const string ScriptPath = "../Docs/Dialogue/00_FULL_SCRIPT.md";
    private const string ScenePath = "Assets/Scenes/CafeTime.unity";
    private const string DialogueFolder = "Assets/Data/Dialogue";
    private const string SpeakerFolder = "Assets/Scripts/Customers/CustomerStorage";
    private const string OldGrandmotherPrefab = "Assets/UI/Portraits/GrandmaImage.prefab";
    private const string KitchenArt = "Assets/Art/Backgrounds/BG.png";
    private const string OpeningArt = "Assets/Art/Backgrounds/Mid-Autumn.png";
    private const string EndingMusic = "Assets/BGM/ending.mp3";
    private const string EndingArt = "Assets/Art/Cutscenes/EndingCutscene.png";
    private const string CasesFolder = "Assets/Data/Customers";

    private const int Baker = 1, Mei = 2, Jian = 3, Lina = 4, Artemis = 5, Grandmother = 6, ChangE = 7;

    private static readonly Color NightPanel = new(0.06f, 0.07f, 0.13f, 1f);
    private static readonly Color MemoryPanel = new(0.40f, 0.26f, 0.12f, 1f);

    private static readonly Regex LinePattern =
        new(@"^\*\*\[(?<id>[A-Z0-9-]+) \| (?<speaker>[^\]]+)\]\*\*\s*(?<text>.+)$");
    private static readonly Regex MistakePattern = new(@"^(?<scene>L3|S5|S2B)-(?<cat>[FCSO])\d$");

    private sealed class ScriptLine
    {
        public string Id;
        public string Speaker;
        public string Text;
    }

    [MenuItem("Tools/Last Mooncake/Import Dialogue Script")]
    public static void Import()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Stop Play mode before importing the dialogue.");
            return;
        }

        string scriptFile = Path.GetFullPath(Path.Combine(Application.dataPath, ScriptPath));
        List<ScriptLine> script = Parse(scriptFile);
        AssetDatabase.Refresh();
        EnsureFolder("Assets/Data", "Dialogue");

        Dictionary<int, Customer> speakers = EnsureSpeakers();
        Sprite endingSprite = LoadSprite(EndingArt);

        // Opening and ending.
        Conversation opening = Build("00_Opening", script, id => id.StartsWith("S1-"));
        SetPanel(opening, NightPanel, string.Empty);
        opening.panelImage = LoadSprite(OpeningArt);
        Conversation ending = Build("99_Ending", script, id => id.StartsWith("S8-"));
        SetPanel(ending, NightPanel, "A memory of the moon");
        SetPanelImageFrom(ending, "S8-07", endingSprite, script);

        // Customer 1: Lina (guided tutorial).
        Conversation linaArrival = Build("10_Lina_Arrival", script, id => id.StartsWith("L1-") || id.StartsWith("L2-"));
        Conversation linaGuide = Build("11_Lina_RecipeGuide", script, id => Regex.IsMatch(id, @"^L3-\d\d[A-Z]?$"));
        Conversation linaResolution = Build("12_Lina_Resolution", script, id => id.StartsWith("L4-"));

        // Customer 2: Mei arrives alone and makes the cake she remembers (no centre).
        Conversation meiArrival = Build("20_Mei_Arrival", script, id => id.StartsWith("S2-"));
        Conversation meiGuide = Build("21_Mei_RecipeGuide", script, id => Regex.IsMatch(id, @"^S2B-\d\d$"));
        Conversation meiResolution = Build("22_Mei_Resolution", script, id => id.StartsWith("S2C-"));

        // Customer 3: Jian arrives separately and completes the recipe with the yolk.
        Conversation jianArrival = Build("30_Jian_Arrival", script, id => id.StartsWith("S3-"));
        Conversation twinsGuide = Build("31_Twins_RecipeGuide", script, id => id.StartsWith("S4-"));
        Conversation twinsMemory = Build("32_Twins_Memory", script, id => id.StartsWith("S6-"));
        SetPanel(twinsMemory, MemoryPanel, "Grandmother's kitchen, years ago");
        twinsMemory.panelImage = LoadSprite(KitchenArt);
        Conversation twinsResolution = Build("33_Twins_Resolution", script, id => id.StartsWith("S7-"));
        DeleteStaleConversations("21_Jian_Arrival", "22_Twins_RecipeGuide", "23_Twins_Memory", "24_Twins_Resolution");

        CustomerCase linaCase = AssetDatabase.LoadAssetAtPath<CustomerCase>($"{CasesFolder}/01_Lina.asset");
        CustomerCase meiCase = EnsureMeiCase();
        CustomerCase jianCase = EnsureJianCase();
        if (linaCase == null || jianCase == null)
        {
            throw new InvalidOperationException("Customer cases are missing. Run Tools > Last Mooncake > Configure Recipe Scene first.");
        }

        linaCase.EditorSetDialogue(new[] { linaArrival }, linaGuide, new[] { linaResolution });
        ConfigureClues(linaCase, "L3", "Lina", script, notebookIds: new[] { "L3-01", "L3-02", "L3-03", "L3-04" });
        EditorUtility.SetDirty(linaCase);

        meiCase.EditorSetDialogue(new[] { meiArrival }, meiGuide, new[] { meiResolution });
        ConfigureClues(meiCase, "S2B", "Mei", script, notebookIds: new[] { "S2B-N1", "S2B-N2", "S2B-N3", "S2B-N4" });
        EditorUtility.SetDirty(meiCase);

        // Notebook ids in category order: filling, centre, sweetness, finish.
        jianCase.EditorSetDialogue(new[] { jianArrival }, twinsGuide, new[] { twinsMemory, twinsResolution });
        ConfigureClues(jianCase, "S5", "Twins", script, notebookIds: new[] { "S4-02", "S4-04", "S4-03", "S4-05" });
        EditorUtility.SetDirty(jianCase);

        AssetDatabase.SaveAssets();
        WireScene(speakers, opening, ending, linaCase, meiCase, jianCase);
        Debug.Log($"Imported {script.Count} script lines into {DialogueFolder} and wired {ScenePath}.");
    }

    // ---------------------------------------------------------------- customer cases

    private static CustomerCase EnsureMeiCase()
    {
        string path = $"{CasesFolder}/02_Mei.asset";
        CustomerCase existing = AssetDatabase.LoadAssetAtPath<CustomerCase>(path);
        if (existing != null)
        {
            return existing;
        }

        // Memory notes and corrections are filled from the script by ConfigureClues.
        CustomerCase created = ScriptableObject.CreateInstance<CustomerCase>();
        created.EditorSetup(
            "Mei",
            "The cake tastes close to Grandmother's, but its centre is hollow. Something is still missing.",
            new CaseClue(RecipeCategory.Filling, RecipeChoice.Lotus, string.Empty,
                "Hint: Think about which seeds Mei ground.", string.Empty),
            new CaseClue(RecipeCategory.Centre, RecipeChoice.NoYolk, string.Empty,
                "Hint: Make only what Mei actually remembers.", string.Empty),
            new CaseClue(RecipeCategory.Sweetness, RecipeChoice.LowSweetness, string.Empty,
                "Hint: One small spoonful. Never more.", string.Empty),
            new CaseClue(RecipeCategory.Finish, RecipeChoice.Osmanthus, string.Empty,
                "Hint: Recall the scent on Grandmother's sleeves.", string.Empty));
        AssetDatabase.CreateAsset(created, path);
        return created;
    }

    /// <summary>The old shared twins case becomes Jian's case (same recipe, with the yolk).</summary>
    private static CustomerCase EnsureJianCase()
    {
        string path = $"{CasesFolder}/03_Jian.asset";
        string oldPath = $"{CasesFolder}/03_MeiAndJian.asset";
        if (AssetDatabase.LoadAssetAtPath<CustomerCase>(path) == null && AssetDatabase.LoadAssetAtPath<CustomerCase>(oldPath) != null)
        {
            string error = AssetDatabase.MoveAsset(oldPath, path);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(error);
            }
        }

        CustomerCase jian = AssetDatabase.LoadAssetAtPath<CustomerCase>(path);
        if (jian != null && jian.CustomerName != "Jian")
        {
            var clues = new List<CaseClue>(jian.Clues);
            jian.EditorSetup("Jian", jian.ResolutionSummary, clues.ToArray());
        }

        return jian;
    }

    private static void DeleteStaleConversations(params string[] assetNames)
    {
        foreach (string assetName in assetNames)
        {
            string path = $"{DialogueFolder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<Conversation>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
    }

    // ---------------------------------------------------------------- parsing

    private static List<ScriptLine> Parse(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Dialogue script not found.", path);
        }

        var lines = new List<ScriptLine>();
        foreach (string raw in File.ReadAllLines(path))
        {
            Match match = LinePattern.Match(raw.Trim());
            if (match.Success)
            {
                lines.Add(new ScriptLine
                {
                    Id = match.Groups["id"].Value,
                    Speaker = match.Groups["speaker"].Value.Trim(),
                    Text = Regex.Replace(match.Groups["text"].Value.Trim(), @"\*\*(.+?)\*\*", "<b>$1</b>")
                });
            }
        }

        return lines;
    }

    /// <summary>Maps a script speaker tag to an NPC id and style. Returns false for lines that are not dialogue.</summary>
    private static bool TryMapSpeaker(string tag, out DialogueLine line)
    {
        line = new DialogueLine();
        string speaker = tag.ToLowerInvariant();

        if (speaker == "tutorial" || speaker.Contains("notebook"))
        {
            return false;
        }

        if (speaker == "stage")
        {
            line.npcId = 0;
            line.style = LineStyle.Stage;
        }
        else if (speaker.StartsWith("artemis"))
        {
            line.npcId = Artemis;
            line.style = speaker.Contains("memory") ? LineStyle.Remembered : LineStyle.Thought;
        }
        else if (speaker.StartsWith("grandmother"))
        {
            line.npcId = Grandmother;
            line.style = speaker.Contains("remembered") ? LineStyle.Remembered : LineStyle.Speech;
        }
        else if (speaker.StartsWith("chang"))
        {
            line.npcId = ChangE;
            line.style = speaker.Contains("memory") ? LineStyle.Remembered : LineStyle.Speech;
        }
        else if (speaker == "young mei")
        {
            line.npcId = Mei;
            line.speakerName = "Young Mei";
        }
        else if (speaker == "young jian")
        {
            line.npcId = Jian;
            line.speakerName = "Young Jian";
        }
        else if (speaker == "baker") line.npcId = Baker;
        else if (speaker == "mei") line.npcId = Mei;
        else if (speaker == "jian") line.npcId = Jian;
        else if (speaker == "lina") line.npcId = Lina;
        else
        {
            Debug.LogWarning($"Unknown speaker '{tag}' in the dialogue script; line skipped.");
            return false;
        }

        return true;
    }

    // ---------------------------------------------------------------- conversations

    private static Conversation Build(string assetName, List<ScriptLine> script, Func<string, bool> include)
    {
        var lines = new List<DialogueLine>();
        foreach (ScriptLine scriptLine in script)
        {
            if (include(scriptLine.Id) && TryMapSpeaker(scriptLine.Speaker, out DialogueLine line))
            {
                line.text = scriptLine.Text;
                lines.Add(line);
            }
        }

        return SaveConversation(assetName, lines);
    }

    private static Conversation SaveConversation(string assetName, List<DialogueLine> lines)
    {
        string path = $"{DialogueFolder}/{assetName}.asset";
        Conversation conversation = AssetDatabase.LoadAssetAtPath<Conversation>(path);
        if (conversation == null)
        {
            conversation = ScriptableObject.CreateInstance<Conversation>();
            AssetDatabase.CreateAsset(conversation, path);
        }

        conversation.Lines = lines;
        conversation.showPanel = false;
        conversation.panelImage = null;
        conversation.panelTitle = string.Empty;
        EditorUtility.SetDirty(conversation);
        return conversation;
    }

    private static void SetPanel(Conversation conversation, Color color, string title)
    {
        conversation.showPanel = true;
        conversation.panelColor = color;
        conversation.panelTitle = title;
        EditorUtility.SetDirty(conversation);
    }

    /// <summary>Switches the panel picture on the line that came from the given script id.</summary>
    private static void SetPanelImageFrom(Conversation conversation, string scriptId, Sprite sprite, List<ScriptLine> script)
    {
        ScriptLine source = script.Find(line => line.Id == scriptId);
        DialogueLine target = source != null ? conversation.Lines.Find(line => line.text == source.Text) : null;
        if (target == null || sprite == null)
        {
            Debug.LogWarning($"Could not attach the panel picture to {scriptId}.");
            return;
        }

        target.panelImage = sprite;
        EditorUtility.SetDirty(conversation);
    }

    /// <summary>Memory notes, wrong-recipe reactions and direct hints for each recipe category.</summary>
    private static void ConfigureClues(CustomerCase customerCase, string mistakeScene, string prefix, List<ScriptLine> script, string[] notebookIds)
    {
        var categories = new Dictionary<string, RecipeCategory>
        {
            ["F"] = RecipeCategory.Filling,
            ["C"] = RecipeCategory.Centre,
            ["S"] = RecipeCategory.Sweetness,
            ["O"] = RecipeCategory.Finish
        };

        int noteIndex = 0;
        foreach (KeyValuePair<string, RecipeCategory> pair in categories)
        {
            CaseClue clue = customerCase.GetClue(pair.Value);
            if (clue == null)
            {
                continue;
            }

            var reaction = new List<DialogueLine>();
            var hint = new List<DialogueLine>();
            string hintText = clue.Correction;
            foreach (ScriptLine scriptLine in script)
            {
                Match match = MistakePattern.Match(scriptLine.Id);
                if (!match.Success || match.Groups["scene"].Value != mistakeScene || match.Groups["cat"].Value != pair.Key)
                {
                    continue;
                }

                if (!TryMapSpeaker(scriptLine.Speaker, out DialogueLine line))
                {
                    continue;
                }

                line.text = scriptLine.Text;
                if (scriptLine.Speaker.ToLowerInvariant().Contains("direct hint"))
                {
                    hint.Add(line);
                    hintText = scriptLine.Text;
                }
                else
                {
                    reaction.Add(line);
                }
            }

            string note = script.Find(line => line.Id == notebookIds[noteIndex])?.Text ?? clue.MemoryNote;
            noteIndex++;

            clue.EditorSetText(note, hintText);
            clue.EditorSetDialogue(
                SaveConversation($"{prefix}_Wrong_{pair.Value}", reaction),
                SaveConversation($"{prefix}_Hint_{pair.Value}", hint));
        }
    }

    // ---------------------------------------------------------------- speakers

    private static Dictionary<int, Customer> EnsureSpeakers()
    {
        var speakers = new Dictionary<int, Customer>();
        foreach (string guid in AssetDatabase.FindAssets("t:Customer", new[] { SpeakerFolder }))
        {
            Customer customer = AssetDatabase.LoadAssetAtPath<Customer>(AssetDatabase.GUIDToAssetPath(guid));
            if (customer != null)
            {
                speakers[customer.npcId] = customer;
            }
        }

        AudioClip bakerVoice = speakers.TryGetValue(Baker, out Customer baker) ? baker.TextSFX : null;

        // New speakers only; existing assets keep whatever the team set in the Inspector.
        EnsureSpeaker(speakers, Artemis, "Artemis", null, null);
        EnsureSpeaker(speakers, Grandmother, "Grandmother", null, bakerVoice);
        EnsureSpeaker(speakers, ChangE, "Chang'e", null, null);

        // The twins' Grandmother has no portrait (Grandma.png is the Baker).
        Customer grandmother = speakers[Grandmother];
        if (grandmother.CharacterPortrait != null || grandmother.OpenMouthPortrait != null)
        {
            grandmother.CharacterPortrait = null;
            grandmother.OpenMouthPortrait = null;
            EditorUtility.SetDirty(grandmother);
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(OldGrandmotherPrefab) != null)
        {
            AssetDatabase.DeleteAsset(OldGrandmotherPrefab);
        }

        return speakers;
    }

    private static void EnsureSpeaker(Dictionary<int, Customer> speakers, int id, string speakerName, Image portrait, AudioClip voice)
    {
        if (speakers.ContainsKey(id))
        {
            return;
        }

        Customer customer = ScriptableObject.CreateInstance<Customer>();
        customer.npcId = id;
        customer.Name = speakerName;
        customer.CharacterPortrait = portrait;
        customer.OpenMouthPortrait = null; // only one frame exists for these speakers
        customer.TextSFX = voice;
        AssetDatabase.CreateAsset(customer, $"{SpeakerFolder}/{id}.asset");
        speakers[id] = customer;
    }

    private static Sprite LoadSprite(string path)
    {
        if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
        {
            return null;
        }

        if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ---------------------------------------------------------------- scene

    private static void WireScene(Dictionary<int, Customer> speakers, Conversation opening, Conversation ending, params CustomerCase[] cases)
    {
        var casePaths = new List<string>();
        foreach (CustomerCase customerCase in cases)
        {
            casePaths.Add(AssetDatabase.GetAssetPath(customerCase));
        }

        string openingPath = AssetDatabase.GetAssetPath(opening);
        string endingPath = AssetDatabase.GetAssetPath(ending);
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Opening a scene unloads assets nothing references yet, so reload them from disk.
        opening = AssetDatabase.LoadAssetAtPath<Conversation>(openingPath);
        ending = AssetDatabase.LoadAssetAtPath<Conversation>(endingPath);
        for (int index = 0; index < cases.Length; index++)
        {
            cases[index] = AssetDatabase.LoadAssetAtPath<CustomerCase>(casePaths[index]);
        }

        DialogueManager dialogue = FindInScene<DialogueManager>(scene);
        CustomerManager customers = FindInScene<CustomerManager>(scene);
        CafeSession session = FindInScene<CafeSession>(scene);
        if (dialogue == null || customers == null || session == null)
        {
            throw new InvalidOperationException("CafeTime needs a DialogueManager, CustomerManager and CafeSession.");
        }

        var ordered = new List<Customer>();
        foreach (int id in new[] { Baker, Mei, Jian, Lina, Artemis, Grandmother, ChangE })
        {
            if (speakers.TryGetValue(id, out Customer customer))
            {
                ordered.Add(customer);
            }
        }

        customers.NPCs = ordered;
        EditorUtility.SetDirty(customers);

        StoryPanel panel = EnsureStoryPanel(dialogue);
        var serializedDialogue = new SerializedObject(dialogue);
        serializedDialogue.FindProperty("storyPanel").objectReferenceValue = panel;
        serializedDialogue.ApplyModifiedPropertiesWithoutUndo();

        var serializedSession = new SerializedObject(session);
        serializedSession.FindProperty("DM").objectReferenceValue = dialogue;
        serializedSession.FindProperty("opening").objectReferenceValue = opening;
        serializedSession.FindProperty("ending").objectReferenceValue = ending;
        serializedSession.FindProperty("endingMusic").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(EndingMusic);
        SerializedProperty caseList = serializedSession.FindProperty("cases");
        caseList.arraySize = cases.Length;
        for (int index = 0; index < cases.Length; index++)
        {
            caseList.GetArrayElementAtIndex(index).objectReferenceValue = cases[index];
        }

        serializedSession.ApplyModifiedPropertiesWithoutUndo();

        // End credits sit above the story UI but below the pause menu.
        var flow = (GameFlowController)serializedSession.FindProperty("flow").objectReferenceValue;
        Transform canvas = dialogue.TextBox.transform.parent;
        EndingCreditsSetup.Build(canvas, session.gameObject, flow).transform.SetAsLastSibling();
        canvas.Find("EndingPanel").gameObject.SetActive(false);
        canvas.Find("PausePanel")?.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static StoryPanel EnsureStoryPanel(DialogueManager dialogue)
    {
        Transform dialogueBox = dialogue.TextBox.transform;
        Transform canvas = dialogueBox.parent;

        Transform existing = canvas.Find("StoryPanel");
        GameObject panelObject = existing != null
            ? existing.gameObject
            : new GameObject("StoryPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(StoryPanel));
        panelObject.transform.SetParent(canvas, false);
        panelObject.layer = dialogueBox.gameObject.layer;
        Stretch(panelObject.GetComponent<RectTransform>());

        Image picture = EnsureChild(panelObject.transform, "Picture", typeof(Image)).GetComponent<Image>();
        Stretch(picture.rectTransform);
        picture.preserveAspect = true;
        picture.raycastTarget = false;

        Image titleBand = EnsureChild(panelObject.transform, "TitleBand", typeof(Image)).GetComponent<Image>();
        RectTransform bandRect = titleBand.rectTransform;
        bandRect.anchorMin = new Vector2(0f, 0.5f);
        bandRect.anchorMax = new Vector2(1f, 0.5f);
        bandRect.pivot = new Vector2(0.5f, 0.5f);
        bandRect.anchoredPosition = new Vector2(0f, 220f);
        bandRect.sizeDelta = new Vector2(0f, 130f);
        titleBand.color = new Color(0f, 0f, 0f, 0.55f);
        titleBand.raycastTarget = false;
        titleBand.transform.SetSiblingIndex(picture.transform.GetSiblingIndex() + 1);

        TextMeshProUGUI title = EnsureChild(panelObject.transform, "Title", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        title.transform.SetAsLastSibling();
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 220f);
        titleRect.sizeDelta = new Vector2(1400f, 120f);
        title.fontSize = 60f;
        title.fontStyle = FontStyles.Italic;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(1f, 0.86f, 0.55f);
        title.raycastTarget = false;

        StoryPanel panel = panelObject.GetComponent<StoryPanel>();
        var serialized = new SerializedObject(panel);
        serialized.FindProperty("group").objectReferenceValue = panelObject.GetComponent<CanvasGroup>();
        serialized.FindProperty("background").objectReferenceValue = panelObject.GetComponent<Image>();
        serialized.FindProperty("picture").objectReferenceValue = picture;
        serialized.FindProperty("title").objectReferenceValue = title;
        serialized.FindProperty("titleBand").objectReferenceValue = titleBand.gameObject;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        // Draw order: gameplay UI < story panel < dialogue box < notes / ending / pause overlays.
        panelObject.transform.SetAsLastSibling();
        dialogueBox.SetAsLastSibling();
        foreach (string overlay in new[] { "MemoryNotesPanel", "EndingPanel", "PausePanel" })
        {
            canvas.Find(overlay)?.SetAsLastSibling();
        }

        panelObject.SetActive(false);
        EditorUtility.SetDirty(panelObject);
        return panel;
    }

    private static GameObject EnsureChild(Transform parent, string name, Type component)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        var child = new GameObject(name, typeof(RectTransform), component);
        child.transform.SetParent(parent, false);
        child.layer = parent.gameObject.layer;
        return child;
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

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T found = root.GetComponentInChildren<T>(true);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
