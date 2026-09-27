using System;
using System.Collections.Generic;
using UnityEngine;

public enum LineStyle
{
    Speech,     // spoken aloud: name, portrait, mouth flaps
    Thought,    // Artemis's thoughts: italic, no voice
    Remembered, // a voice from memory (Grandmother, Chang'e): italic
    Stage       // narration / stage direction: no name or portrait
}

[Serializable]
public class DialogueLine
{
    public string text;
    public int npcId;
    public LineStyle style = LineStyle.Speech;
    [Tooltip("Optional: shown instead of the NPC's name (e.g. \"Young Mei\").")]
    public string speakerName;
    [Tooltip("Hide the speaker's portrait on this line (e.g. Chang'e's voice before she is revealed).")]
    public bool hidePortrait;
    [Tooltip("Optional: switches the story panel to this picture from this line on.")]
    public Sprite panelImage;
}

[CreateAssetMenu(fileName = "Conversation", menuName = "Dialogue/Conversation")]


public class Conversation : ScriptableObject
{
    [Header("Story panel (full-screen, behind the dialogue box)")]
    public bool showPanel;
    public Sprite panelImage;
    public Color panelColor = new(0.06f, 0.07f, 0.13f, 1f);
    public string panelTitle;

    public List<DialogueLine> Lines = new();
}
