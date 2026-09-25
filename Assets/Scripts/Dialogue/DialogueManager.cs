
using UnityEngine;
using System.Collections;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] CustomerManager CM;
    public TextMeshProUGUI TextDisplay;
    public GameObject TextBox;
    public AudioClip TalkSfx;
    public AudioSource AS;
    bool doLerp = false;
    Vector3 OgPos;
    Vector3 TargetPos;
    public string Text;
    RectTransform rectTransform;
    float t = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        StartCoroutine(DropDialogueBox());
    }
    public void Rise() //test for dialogue
    {
        TextBox.SetActive(true);
        StartCoroutine(RiseDialogueBox());
    }

    public void Conversation1Test(Conversation Convo1) //test for dialogue
    {
        StartCoroutine(TalkConversation(Convo1, TextDisplay));
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
                yield return new WaitForSeconds(0.035f);
        }

        yield return new WaitForSeconds(1f);
    }

    IEnumerator TalkConversation(Conversation Convo, TextMeshProUGUI text)
    {
        for (int z = 0; z < Convo.Lines.Count; z++)
        {
            TalkSfx = CM.FindNPC(Convo.Lines[z].npcId).TextSFX;
            string dialogue = CM.FindNPC(Convo.Lines[z].npcId).Name + ": ";
            dialogue += Convo.Lines[z].text;
            string talking = "";
            for (int i = 0; i < dialogue.Length; i++)
            {
                talking += dialogue[i];
                text.text = talking;
                AS.PlayOneShot(TalkSfx);
                if (dialogue[i] == ',' || dialogue[i] == '.') 
                    yield return new WaitForSeconds(0.2f);
                else
                    yield return new WaitForSeconds(0.035f);
            }

            yield return new WaitForSeconds(1f);
        }
    }
    public void TypewriterEffect(string dialogue, TextMeshProUGUI text)
    {
        StartCoroutine(Talk(dialogue, text));
    }
}
