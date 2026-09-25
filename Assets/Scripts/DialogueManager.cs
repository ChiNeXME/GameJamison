
using UnityEngine;
using System.Collections;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI test;
    public AudioClip TalkSfx;
    public AudioSource AS;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TypewriterEffect("HELLO!, im just testing my dialogue thingy", test);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Talk(string dialogue, TextMeshProUGUI text)
    {
        string talking = "";
        for (int i = 0; i < dialogue.Length; i++)
        {
            
            talking += dialogue[i];
            text.text = talking;
            AS.PlayOneShot(TalkSfx);
            if (dialogue[i] == ',' || dialogue[i] == '.') 
                yield return new WaitForSeconds(0.2f);
            else
                yield return new WaitForSeconds(0.05f);
        }
    }
    public void TypewriterEffect(string dialogue, TextMeshProUGUI text)
    {
        StartCoroutine(Talk(dialogue, text));
    }
}
