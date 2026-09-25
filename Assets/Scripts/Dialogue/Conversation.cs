using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class DialogueLine
{
    public string text;
    public int npcId;
}

[CreateAssetMenu(fileName = "Conversation", menuName = "Dialogue/Conversation")]


public class Conversation : ScriptableObject
{
    public List<DialogueLine> Lines = new();
}
