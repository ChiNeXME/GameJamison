using System.Collections.Generic;
using UnityEngine;

public class DialogueHolder : MonoBehaviour
{
    public static DialogueHolder instance;
    public List<Conversation> conversations;
    void Awake()
    {
        if (instance == null)
        { 
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}
