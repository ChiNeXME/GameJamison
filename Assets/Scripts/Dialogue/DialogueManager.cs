
using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] CustomerManager CM;
    [SerializeField] Image DialogueImage;
    [SerializeField] StoryPanel storyPanel;
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
    bool SkipDialogue;
    bool isTalking;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // The SFX player lives in the main menu scene; it is missing when CafeTime is played directly.
        GameObject sfxPlayer = GameObject.Find("SFXPlayer");
        AS = sfxPlayer != null ? sfxPlayer.GetComponent<AudioSource>() : null;
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

        if (AdvancePressed())
        {
            SkipDialogue = true;
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
        TextDisplay.text = "";
        DialogueImage.enabled = false;
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

    IEnumerator MouthMoving(Image i1, Image i2)
    {
        while (isTalking)
        {
            DialogueImage.sprite = i2.sprite;
            yield return new WaitForSeconds(0.1f);
            DialogueImage.sprite = i1.sprite;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Space or left click advances; ignored while the game is paused.
    bool AdvancePressed()
    {
        return Time.timeScale > 0f && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
    }

    string FormatLine(DialogueLine line, Customer speaker)
    {
        string name = !string.IsNullOrEmpty(line.speakerName) ? line.speakerName
            : speaker != null ? speaker.Name : "";
        switch (line.style)
        {
            case LineStyle.Stage:
                return "<i>" + line.text + "</i>";
            case LineStyle.Thought:
                return "<b>" + name + "</b> <size=75%>(thinking)</size>: <i>" + line.text + "</i>";
            case LineStyle.Remembered:
                return "<b>" + name + "</b> <size=75%>(memory)</size>: <i>" + line.text + "</i>";
            default:
                return "<b>" + name + "</b>: " + line.text;
        }
    }

    IEnumerator TalkConversation(Conversation Convo, TextMeshProUGUI text)
    {
        if (Convo.showPanel && storyPanel != null)
            yield return storyPanel.Show(Convo.panelColor, Convo.panelImage, Convo.panelTitle);

        for (int z = 0; z < Convo.Lines.Count; z++)
        {
            DialogueLine line = Convo.Lines[z];
            Customer speaker = line.style == LineStyle.Stage ? null : CM.FindNPC(line.npcId);
            if (line.panelImage != null && storyPanel != null)
                storyPanel.ChangePicture(line.panelImage);

            SkipDialogue = false;
            TalkSfx = speaker != null && line.style == LineStyle.Speech ? speaker.TextSFX : null;
            text.text = FormatLine(line, speaker);
            text.maxVisibleCharacters = 0;
            text.ForceMeshUpdate();
            int count = text.textInfo.characterCount;

            bool hasPortrait = speaker != null && speaker.CharacterPortrait != null;
            DialogueImage.enabled = hasPortrait;
            if (hasPortrait)
                DialogueImage.sprite = speaker.CharacterPortrait.sprite;

            isTalking = true;
            if (hasPortrait && speaker.OpenMouthPortrait != null && line.style != LineStyle.Thought)
                StartCoroutine(MouthMoving(speaker.CharacterPortrait, speaker.OpenMouthPortrait));
            for (int i = 0; i < count; i++)
            {
                if (SkipDialogue)
                    break;
                char c = text.textInfo.characterInfo[i].character;
                text.maxVisibleCharacters = i + 1;
                if (c != ' ' && TalkSfx != null && AS != null)
                    AS.PlayOneShot(TalkSfx);

                if (c == ',' || c == '.')
                    yield return new WaitForSeconds(0.2f);
                else
                    yield return new WaitForSeconds(0.035f);
            }
            text.maxVisibleCharacters = 99999;
            isTalking = false;
            yield return new WaitForSeconds(0.2f);
            yield return new WaitUntil(AdvancePressed);
            yield return null;
        }

        if (Convo.showPanel && storyPanel != null)
            yield return storyPanel.Hide();
    }
}
