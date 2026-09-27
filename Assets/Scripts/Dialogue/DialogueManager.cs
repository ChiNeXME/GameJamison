
using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] CustomerManager CM;
    public TextMeshProUGUI TextDisplay;
    public GameObject TextBox;
    public AudioClip TalkSfx;
    AudioSource AS;
    bool doLerp = false;
    Vector3 OgPos;
    Vector3 TargetPos;
    public string Text;
    RectTransform rectTransform;
    float t = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AS = GameObject.Find("SFXPlayer").GetComponent<AudioSource>();
        rectTransform = TextBox.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (doLerp)
        {
            t += Time.deltaTime * 3;
            rectTransform.anchoredPosition = Vector3.Lerp(OgPos, TargetPos, t);
        }
    }

    public void Drop() //test for dialogue
    {
        t = 0;
        StartCoroutine(DropDialogueBox());
    }
    public void Rise() //test for dialogue
    {
        t = 0;
        TextBox.SetActive(true);
        StartCoroutine(RiseDialogueBox());
    }

    public Coroutine ConversationStart(Conversation Convo) //test for dialogue
    {
        return StartCoroutine(TalkConversation(Convo, TextDisplay));
    }

    IEnumerator RiseDialogueBox()
    {
        //set from a bit below screen pos
        doLerp = true;
        rectTransform.anchoredPosition  = new Vector3(0,-870,0);
        OgPos = rectTransform.anchoredPosition;
        TargetPos = new Vector3(0,-302,0);

        yield return new WaitForSeconds(0.5f);
        doLerp = false;
        t = 0f;
    }

    IEnumerator DropDialogueBox()
    {
        doLerp = true;
        rectTransform.anchoredPosition  = new Vector3(0,-302,0);
        OgPos = rectTransform.anchoredPosition;
        TargetPos = new Vector3(0,-870,0);
        yield return new WaitForSeconds(0.5f);
        //Disables
        doLerp = false;
        TextBox.SetActive(false);
        t = 0f;
    }

    IEnumerator TalkConversation(Conversation Convo, TextMeshProUGUI text)
    {
        for (int z = 0; z < Convo.Lines.Count; z++)
        {
            bool Skip = false;

            TalkSfx = CM.FindNPC(Convo.Lines[z].npcId).TextSFX;
            string dialogue = CM.FindNPC(Convo.Lines[z].npcId).Name + ": ";
            dialogue += Convo.Lines[z].text;
            string talking = "";

            for (int i = 0; i < dialogue.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    text.text = dialogue;
                    Skip = true;
                    break;
                }
                talking += dialogue[i];
                text.text = talking;
                if (dialogue[i] != ' ')
                    AS.PlayOneShot(TalkSfx);

                if (dialogue[i] == ',' || dialogue[i] == '.') 
                    yield return new WaitForSeconds(0.2f);
                else
                    yield return new WaitForSeconds(0.035f);
                
            }

            if (Skip)
                yield return null;
            //yield return new WaitForSeconds(0.2f);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            yield return null;
        }
    }
}
